using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class Product : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public ProductCategory Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string ShortDescriptionEn { get; set; } = string.Empty;

    // Default pricing — used directly for products without the variants
    public decimal RetailPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public int? MinimumWholesaleQuantity { get; set; }

    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}