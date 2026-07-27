namespace AYIGroupMarket.Application.DTOs;

public record ProductCategoryDto(
    Guid Id,
    string Name,
    string NameEn,
    string Slug,
    string Icon,
    bool IsActive,
    int DisplayOrder);

public record ProductListItemDto(
    Guid Id,
    string Sku,
    string Slug,
    string Name,
    string NameEn,
    string ShortDescription,
    string ShortDescriptionEn,
    decimal RetailPrice,
    bool IsFeatured,
    string? PrimaryImageUrl,
    Guid CategoryId,
    string CategoryName,
    string CategoryNameEn);

public record ProductVariantDto(
    Guid Id,
    string Sku,
    string Name,
    string NameEn,
    List<ProductPriceDto> Prices);

public record ProductPriceDto(
    string PriceType,
    decimal Amount,
    int? MinimumQuantity);

public record ProductDetailDto(
    Guid Id,
    string Sku,
    string Slug,
    string Name,
    string NameEn,
    string Description,
    string DescriptionEn,
    decimal RetailPrice,
    decimal? WholesalePrice,
    int? MinimumWholesaleQuantity,
    int StockQuantity,
    bool IsFeatured,
    List<string> ImageUrls,
    List<ProductVariantDto> Variants,
    Guid CategoryId,
    string CategoryName,
    string CategoryNameEn);

public record ProductPricingDto(
    Guid ProductId,
    decimal RetailPrice,
    decimal? WholesalePrice,      // null unless the caller is verified wholesale
    int? MinimumWholesaleQuantity); // null unless the caller is verified wholesale