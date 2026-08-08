using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Reviews.GetProductReviews;

public record GetProductReviewsQuery(Guid ProductId) : IRequest<ReviewSummaryDto>;

public class GetProductReviewsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductReviewsQuery, ReviewSummaryDto>
{
    public async Task<ReviewSummaryDto> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        // Only ever return Approved reviews to the storefront — Pending/Rejected stay admin-only
        var reviews = await db.ProductReviews.AsNoTracking()
            .Where(r => r.ProductId == request.ProductId && r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ProductReviewDto(
                r.Id, r.UserDisplayName, r.Rating, r.Title, r.Comment, r.IsVerifiedPurchase, r.CreatedAt))
            .ToListAsync(cancellationToken);

        var average = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;

        return new ReviewSummaryDto(Math.Round(average, 1), reviews.Count, reviews);
    }
}