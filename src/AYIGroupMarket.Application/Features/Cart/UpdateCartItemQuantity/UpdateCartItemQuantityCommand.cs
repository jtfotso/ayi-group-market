using AYIGroupMarket.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Cart.UpdateCartItemQuantity;

public record UpdateCartItemQuantityCommand(string OwnerKey, Guid CartItemId, int Quantity) : IRequest;

public class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class UpdateCartItemQuantityCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCartItemQuantityCommand>
{
    public async Task Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var item = await db.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == request.CartItemId && i.Cart.OwnerKey == request.OwnerKey, cancellationToken)
            ?? throw new KeyNotFoundException("Cart item not found");

        item.Quantity = request.Quantity;
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}