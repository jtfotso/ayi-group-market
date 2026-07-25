using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Cart.RemoveCartItem;

public record RemoveCartItemCommand(string OwnerKey, Guid CartItemId) : IRequest;

public class RemoveCartItemCommandHandler(IApplicationDbContext db) : IRequestHandler<RemoveCartItemCommand>
{
    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == request.CartItemId && i.Cart.OwnerKey == request.OwnerKey, cancellationToken)
            ?? throw new KeyNotFoundException("Cart item not found");

        db.CartItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
    }
}