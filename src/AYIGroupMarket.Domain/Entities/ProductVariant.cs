using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;    // e.g. "33 cl", "Carton 24 unités"
    public string NameEn { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
}