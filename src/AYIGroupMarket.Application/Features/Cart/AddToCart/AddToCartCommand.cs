using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Cart.AddToCart;

// IsWholesaleAuthorized is derived server-side from the real ClaimsPrincipal role —
// never a client-supplied flag. See ProductDetail.razor for how it's resolved.
public record AddToCartCommand(
    string OwnerKey, Guid ProductId, Guid? VariantId, int Quantity, bool IsWholesaleAuthorized) : IRequest<Guid>;

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
            var priceType = request.IsWholesaleAuthorized ? PriceType.Wholesale : PriceType.Retail;

            var variantPrice = await db.ProductPrices.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductVariantId == request.VariantId.Value && p.PriceType == priceType, cancellationToken);

            if (variantPrice is not null)
            {
                // Server-side minimum quantity enforcement — matches spec section 27 exactly:
                // "MinimumWholesaleQuantity = 5 cartons. A wholesale order with 3 cartons must be rejected."
                if (request.IsWholesaleAuthorized && variantPrice.MinimumQuantity.HasValue
                    && request.Quantity < variantPrice.MinimumQuantity.Value)
                {
                    throw new InvalidOperationException(
                        $"Minimum wholesale quantity for this item is {variantPrice.MinimumQuantity.Value}.");
                }

                unitPrice = variantPrice.Amount;
            }
        }
        else if (request.IsWholesaleAuthorized && product.WholesalePrice.HasValue)
        {
            if (product.MinimumWholesaleQuantity.HasValue && request.Quantity < product.MinimumWholesaleQuantity.Value)
            {
                throw new InvalidOperationException(
                    $"Minimum wholesale quantity for this item is {product.MinimumWholesaleQuantity.Value}.");
            }

            unitPrice = product.WholesalePrice.Value;
        }

        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerKey == request.OwnerKey, cancellationToken);

        if (cart is null)
        {
            cart = new Domain.Entities.Cart { OwnerKey = request.OwnerKey };
            db.Carts.Add(cart);
            await db.SaveChangesAsync(cancellationToken); // ensure Cart.Id exists before referencing it below
        }

        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId && i.ProductVariantId == request.VariantId);

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

        await db.SaveChangesAsync(cancellationToken);
        return cart.Id;
    }
}