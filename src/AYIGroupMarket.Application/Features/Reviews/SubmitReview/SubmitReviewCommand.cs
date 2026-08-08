using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Reviews.SubmitReview;

public record SubmitReviewCommand(
    Guid ProductId,
    string UserId,
    string UserDisplayName,
    int Rating,
    string Title,
    string Comment) : IRequest<Guid>;

public class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}

public class SubmitReviewCommandHandler(IApplicationDbContext db) : IRequestHandler<SubmitReviewCommand, Guid>
{
    public async Task<Guid> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        var alreadyReviewed = await db.ProductReviews
            .AnyAsync(r => r.ProductId == request.ProductId && r.UserId == request.UserId, cancellationToken);

        if (alreadyReviewed)
            throw new InvalidOperationException("You have already reviewed this product.");

        // Verified purchase check: does this user have a Delivered or Paid order containing this exact product?
        // Computed server-side — never trust a client-supplied flag for this.
        var isVerifiedPurchase = await db.Orders
            .Where(o => o.OwnerKey == $"user:{request.UserId}" &&
                        (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Paid))
            .SelectMany(o => o.Items)
            .AnyAsync(i => i.ProductId == request.ProductId, cancellationToken);

        var review = new ProductReview
        {
            ProductId = request.ProductId,
            UserId = request.UserId,
            UserDisplayName = request.UserDisplayName,
            Rating = request.Rating,
            Title = request.Title,
            Comment = request.Comment,
            IsVerifiedPurchase = isVerifiedPurchase,
            Status = ReviewStatus.Pending // always starts pending, per spec's moderation requirement
        };

        db.ProductReviews.Add(review);
        await db.SaveChangesAsync(cancellationToken);
        return review.Id;
    }
}