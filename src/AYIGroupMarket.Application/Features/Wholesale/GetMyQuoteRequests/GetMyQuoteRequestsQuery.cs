using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Wholesale.GetMyQuoteRequests;

public record GetMyQuoteRequestsQuery(string UserId) : IRequest<List<QuoteRequestDto>>;

public class GetMyQuoteRequestsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMyQuoteRequestsQuery, List<QuoteRequestDto>>
{
    public async Task<List<QuoteRequestDto>> Handle(GetMyQuoteRequestsQuery request, CancellationToken cancellationToken)
    {
        var quotes = await db.QuoteRequests.AsNoTracking()
            .Include(q => q.WholesaleAccount)
            .Include(q => q.Items).ThenInclude(i => i.Product)
            .Include(q => q.Items).ThenInclude(i => i.ProductVariant)
            .Where(q => q.UserId == request.UserId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        return quotes.Select(q => new QuoteRequestDto(
            q.Id, q.WholesaleAccount.CompanyName, q.DeliveryLocation, q.Message,
            q.Status.ToString(), q.QuotedTotal, q.CreatedAt,
            q.Items.Select(i => new QuoteRequestItemDto(
                i.Id,
                i.ProductId, i.Product.Name, i.Product.NameEn,
                i.ProductVariant?.Name, i.ProductVariant?.NameEn,
                i.Quantity, i.QuotedUnitPrice
            )).ToList()
        )).ToList();
    }
}