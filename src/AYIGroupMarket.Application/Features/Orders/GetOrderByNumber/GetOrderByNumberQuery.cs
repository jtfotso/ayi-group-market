using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Orders.GetOrderByNumber;

public record GetOrderByNumberQuery(string OrderNumber) : IRequest<OrderDto?>;

public class GetOrderByNumberQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrderByNumberQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByNumberQuery request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == request.OrderNumber, cancellationToken);

        if (order is null)
            return null;

        var itemDtos = order.Items.Select(i => new OrderItemDto(
            i.ProductNameSnapshot, i.ProductNameEnSnapshot,
            i.VariantNameSnapshot, i.VariantNameEnSnapshot,
            i.UnitPrice, i.Quantity, i.LineTotal
        )).ToList();

        return new OrderDto(order.Id, order.OrderNumber, order.Status.ToString(),
            order.Subtotal, order.ShippingFee, order.Total, itemDtos);
    }
}