using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Products.GetProductBySlug;

public record GetProductBySlugQuery(string Slug) : IRequest<ProductDetailDto?>;

public class GetProductBySlugQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductBySlugQuery, ProductDetailDto?>
{
    public async Task<ProductDetailDto?> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants).ThenInclude(v => v.Prices)
            .FirstOrDefaultAsync(p => p.Slug == request.Slug && p.IsActive, cancellationToken);

        if (product is null)
            return null;

        return new ProductDetailDto(
            product.Id, product.Sku, product.Slug, product.Name, product.NameEn,
            product.Description, product.DescriptionEn,
            product.RetailPrice, product.WholesalePrice, product.MinimumWholesaleQuantity,
            product.StockQuantity, product.IsFeatured,
            product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList(),
            product.Variants.OrderBy(v => v.DisplayOrder).Select(v => new ProductVariantDto(
                v.Id, v.Sku, v.Name, v.NameEn,
                v.Prices.Select(pr => new ProductPriceDto(pr.PriceType.ToString(), pr.Amount, pr.MinimumQuantity)).ToList()
            )).ToList(),
            product.CategoryId, product.Category.Name, product.Category.NameEn);
    }
}