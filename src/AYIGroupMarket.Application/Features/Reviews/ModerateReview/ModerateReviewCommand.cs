using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Reviews.ModerateReview;

public record ModerateReviewCommand(Guid ReviewId, ReviewStatus NewStatus) : IRequest;

public class ModerateReviewCommandHandler(IApplicationDbContext db) : IRequestHandler<ModerateReviewCommand>
{
    public async Task Handle(ModerateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await db.ProductReviews.FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken)
            ?? throw new KeyNotFoundException("Review not found");

        review.Status = request.NewStatus;
        await db.SaveChangesAsync(cancellationToken);
    }
}