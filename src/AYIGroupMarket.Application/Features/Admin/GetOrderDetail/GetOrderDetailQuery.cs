using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.GetOrderDetail;

public record GetOrderDetailQuery(Guid OrderId) : IRequest<AdminOrderDetailDto?>;

public class GetOrderDetailQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrderDetailQuery, AdminOrderDetailDto?>
{
    public async Task<AdminOrderDetailDto?> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Address).ThenInclude(a => a.ShippingZone)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return null;

        return new AdminOrderDetailDto(
            order.Id, order.OrderNumber, order.Status.ToString(), order.PaymentMethod.ToString(),
            order.TrackingNumber, order.Notes, order.CustomerConfirmedAt, order.Subtotal, order.ShippingFee, order.Total,
            order.CustomerName, order.CustomerPhone, order.CustomerEmail, order.IsPickup,
            order.Address?.AddressLine, order.Address?.City, order.Address?.ShippingZone?.Name,
            order.Items.Select(i => new OrderItemDto(
                i.ProductNameSnapshot, i.ProductNameEnSnapshot,
                i.VariantNameSnapshot, i.VariantNameEnSnapshot,
                i.UnitPrice, i.Quantity, i.LineTotal
            )).ToList(),
            order.CreatedAt);
    }
}