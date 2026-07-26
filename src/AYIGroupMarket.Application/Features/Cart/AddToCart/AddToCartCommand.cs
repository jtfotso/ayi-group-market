using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Cart.AddToCart;

public record AddToCartCommand(string OwnerKey, Guid ProductId, Guid? VariantId, int Quantity) : IRequest<Guid>;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.OwnerKey).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class AddToCartCommandHandler(IApplicationDbContext db) : IRequestHandler<AddToCartCommand, Guid>
{
    public async Task<Guid> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Product not found");

        decimal unitPrice = product.RetailPrice;

        if (request.VariantId.HasValue)
        {
            var variantPrice = await db.ProductPrices.AsNoTracking()
                .Where(p => p.ProductVariantId == request.VariantId.Value && p.PriceType == PriceType.Retail)
                .Select(p => p.Amount)
                .FirstOrDefaultAsync(cancellationToken);

            if (variantPrice > 0)
                unitPrice = variantPrice;
        }

        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerKey == request.OwnerKey, cancellationToken);
        
        //Console.WriteLine($"[AddToCart] OwnerKey={request.OwnerKey}, ProductId={request.ProductId}, VariantId={request.VariantId}");
        //Console.WriteLine($"[AddToCart] Cart found: {cart is not null}, Cart.Id={cart?.Id}, Items count={cart?.Items.Count ?? 0}");

        if (cart is null)
        {
            cart = new Domain.Entities.Cart { OwnerKey = request.OwnerKey };
            db.Carts.Add(cart);
        }

        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId && i.ProductVariantId == request.VariantId);
        
        //Console.WriteLine($"[AddToCart] existingItem found: {existingItem is not null}, existingItem.Id={existingItem?.Id}");

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var newItem = new Domain.Entities.CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                ProductVariantId = request.VariantId,
                UnitPrice = unitPrice,
                Quantity = request.Quantity
            };

            db.CartItems.Add(newItem);
        }

        // Diagnostic: dump every tracked entity's state before saving
        /* foreach (var entry in db.ChangeTracker.Entries())
        {
            Console.WriteLine($"[AddToCart] Tracked entity: {entry.Entity.GetType().Name}, State={entry.State}, Id={entry.Property("Id").CurrentValue}");
        } */

        await db.SaveChangesAsync(cancellationToken);
        return cart.Id;
    }
}