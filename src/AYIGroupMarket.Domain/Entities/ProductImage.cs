using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public string AltTextEn { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}