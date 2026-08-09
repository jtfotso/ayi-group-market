using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.CancelOrder;

public record CancelOrderCommand(Guid OrderId, string Reason) : IRequest;

public class CancelOrderCommandHandler(IApplicationDbContext db) : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found");

        if (order.Status == OrderStatus.Cancelled)
            return; // already cancelled, nothing to do

        order.Status = OrderStatus.Cancelled;

        foreach (var item in order.Items)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);
            if (product is null) continue;

            product.StockQuantity += item.Quantity;

            db.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = item.ProductId,
                Type = InventoryTransactionType.Cancellation,
                QuantityChange = item.Quantity,
                ResultingStock = product.StockQuantity,
                Reason = $"Order {order.OrderNumber} cancelled: {request.Reason}"
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}