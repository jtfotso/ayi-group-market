using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    // Snapshots at time of order — protects historical orders from later product edits
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductNameEnSnapshot { get; set; } = string.Empty;
    public string? VariantNameSnapshot { get; set; }
    public string? VariantNameEnSnapshot { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}