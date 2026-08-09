using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Products.GetProductForEdit;

public record ProductEditVariantDto(Guid Id, string Sku, string Name, string NameEn, decimal RetailPrice, decimal? WholesalePrice, int? MinimumWholesaleQuantity);
public record ProductEditImageDto(Guid Id, string Url, bool IsPrimary);

public record ProductEditDto(
    Guid Id, string Sku, string Slug, Guid CategoryId, string Name, string NameEn,
    string ShortDescription, string ShortDescriptionEn, string Description, string DescriptionEn,
    decimal RetailPrice, decimal? WholesalePrice, int? MinimumWholesaleQuantity,
    int StockQuantity, bool IsActive, bool IsFeatured,
    List<ProductEditVariantDto> Variants, List<ProductEditImageDto> Images);

public record GetProductForEditQuery(Guid ProductId) : IRequest<ProductEditDto?>;

public class GetProductForEditQueryHandler(IApplicationDbContext db) : IRequestHandler<GetProductForEditQuery, ProductEditDto?>
{
    public async Task<ProductEditDto?> Handle(GetProductForEditQuery request, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking()
            .Include(p => p.Variants).ThenInclude(v => v.Prices)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null) return null;

        return new ProductEditDto(
            product.Id, product.Sku, product.Slug, product.CategoryId, product.Name, product.NameEn,
            product.ShortDescription, product.ShortDescriptionEn, product.Description, product.DescriptionEn,
            product.RetailPrice, product.WholesalePrice, product.MinimumWholesaleQuantity,
            product.StockQuantity, product.IsActive, product.IsFeatured,
            product.Variants.Select(v => new ProductEditVariantDto(
                v.Id, v.Sku, v.Name, v.NameEn,
                v.Prices.Where(p => p.PriceType == Domain.Enums.PriceType.Retail).Select(p => p.Amount).FirstOrDefault(),
                v.Prices.Where(p => p.PriceType == Domain.Enums.PriceType.Wholesale).Select(p => (decimal?)p.Amount).FirstOrDefault(),
                v.Prices.Where(p => p.PriceType == Domain.Enums.PriceType.Wholesale).Select(p => p.MinimumQuantity).FirstOrDefault()
            )).ToList(),
            product.Images.Select(i => new ProductEditImageDto(i.Id, i.Url, i.IsPrimary)).ToList());
    }
}