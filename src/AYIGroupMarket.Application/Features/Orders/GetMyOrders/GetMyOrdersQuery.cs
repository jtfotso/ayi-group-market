using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Orders.GetMyOrders;

public record GetMyOrdersQuery(string OwnerKey) : IRequest<List<OrderDto>>;

public class GetMyOrdersQueryHandler(IApplicationDbContext db) : IRequestHandler<GetMyOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.OwnerKey == request.OwnerKey)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(o => new OrderDto(
            o.Id, o.OrderNumber, o.Status.ToString(), o.Subtotal, o.ShippingFee, o.Total,
            o.Items.Select(i => new OrderItemDto(
                i.ProductNameSnapshot, i.ProductNameEnSnapshot,
                i.VariantNameSnapshot, i.VariantNameEnSnapshot,
                i.UnitPrice, i.Quantity, i.LineTotal
            )).ToList()
        )).ToList();
    }
}