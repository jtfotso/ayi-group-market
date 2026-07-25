using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public decimal UnitPrice { get; set; } // snapshot at time of add — protects against later price changes
    public int Quantity { get; set; }
}