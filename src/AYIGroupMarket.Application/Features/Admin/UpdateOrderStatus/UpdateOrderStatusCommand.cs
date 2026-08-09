using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus, string? TrackingNumber) : IRequest;

public class UpdateOrderStatusCommandHandler(IApplicationDbContext db, INotificationService notificationService)
    : IRequestHandler<UpdateOrderStatusCommand>
{
    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found");

        order.Status = request.NewStatus;
        if (request.TrackingNumber is not null)
            order.TrackingNumber = request.TrackingNumber;

        await db.SaveChangesAsync(cancellationToken);

        if (order.OwnerKey.StartsWith("user:"))
        {
            var userId = order.OwnerKey["user:".Length..];
            var (title, titleEn) = request.NewStatus switch
            {
                OrderStatus.Shipped => ("Commande expédiée", "Order shipped"),
                OrderStatus.Delivered => ("Commande livrée", "Order delivered"),
                OrderStatus.Cancelled => ("Commande annulée", "Order cancelled"),
                _ => ("Statut de commande mis à jour", "Order status updated")
            };

            await notificationService.NotifyAsync(
                userId, title, titleEn,
                $"Commande {order.OrderNumber}", $"Order {order.OrderNumber}",
                "/mon-compte/commandes", cancellationToken);
        }
    }
}