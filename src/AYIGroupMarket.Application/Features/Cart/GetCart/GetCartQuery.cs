using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Cart.GetCart;

public record GetCartQuery(string OwnerKey) : IRequest<CartDto>;

public class GetCartQueryHandler(IApplicationDbContext db) : IRequestHandler<GetCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await db.Carts.AsNoTracking()
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.OwnerKey == request.OwnerKey, cancellationToken);

        if (cart is null)
            return new CartDto(Guid.Empty, [], 0, 0);

        var items = cart.Items.Select(i => new CartItemDto(
            i.Id, i.ProductId, i.Product.Name, i.Product.NameEn, i.Product.Slug,
            i.ProductVariantId, i.ProductVariant?.Name, i.ProductVariant?.NameEn,
            i.Product.Images.Where(img => img.IsPrimary).Select(img => img.Url).FirstOrDefault(),
            i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity
        )).ToList();

        return new CartDto(cart.Id, items, items.Sum(i => i.LineTotal), items.Sum(i => i.Quantity));
    }
}