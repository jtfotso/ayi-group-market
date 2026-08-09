using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.GetOrders;

public record GetOrdersQuery(
    OrderStatus? Status = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 25) : IRequest<PagedResult<AdminOrderListItemDto>>;

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public class GetOrdersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrdersQuery, PagedResult<AdminOrderListItemDto>>
{
    public async Task<PagedResult<AdminOrderListItemDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Orders.AsNoTracking().Include(o => o.Address).AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(o =>
                o.OrderNumber.Contains(term) ||
                o.Address.FullName.Contains(term) ||
                o.Address.Phone.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new AdminOrderListItemDto(
                o.Id, o.OrderNumber, o.Address.FullName, o.Address.Phone,
                o.Status.ToString(), o.Total, o.PaymentMethod.ToString(), o.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminOrderListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
}