using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Orders.ConfirmOrderReceipt;

// OwnerKey is passed in and checked server-side — a customer must only be able to
// confirm receipt of THEIR OWN order, never someone else's by guessing an order Id.
public record ConfirmOrderReceiptCommand(Guid OrderId, string OwnerKey) : IRequest;

public class ConfirmOrderReceiptCommandHandler(IApplicationDbContext db, INotificationService notificationService)
    : IRequestHandler<ConfirmOrderReceiptCommand>
{
    public async Task Handle(ConfirmOrderReceiptCommand request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found");

        if (order.OwnerKey != request.OwnerKey)
            throw new UnauthorizedAccessException("This order does not belong to you.");

        // Only makes sense to confirm once the order has actually been shipped/is ready
        if (order.Status is not (OrderStatus.Shipped or OrderStatus.ReadyForDelivery))
            throw new InvalidOperationException("This order cannot be confirmed as received yet.");

        order.Status = OrderStatus.Delivered;
        order.CustomerConfirmedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        // Notify admins — no per-admin notification system exists yet, so this is a placeholder
        // for a future "notify all Admin role users" capability. For now, admins can see confirmed
        // receipts directly on the Orders page instead.
    }
}