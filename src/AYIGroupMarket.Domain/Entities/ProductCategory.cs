using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class ProductCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty; // emoji or bootstrap-icon class
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true; // false = "Bientôt disponible"

    public ICollection<Product> Products { get; set; } = new List<Product>();
}