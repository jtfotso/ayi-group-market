using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Reviews.GetPendingReviews;

public record GetPendingReviewsQuery : IRequest<List<ProductReview>>;

public class GetPendingReviewsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPendingReviewsQuery, List<ProductReview>>
{
    public async Task<List<ProductReview>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        return await db.ProductReviews.AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.Status == ReviewStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}