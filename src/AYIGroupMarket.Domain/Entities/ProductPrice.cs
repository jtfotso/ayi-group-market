using AYIGroupMarket.Domain.Common;
using AYIGroupMarket.Domain.Enums;

namespace AYIGroupMarket.Domain.Entities;

public class ProductPrice : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public PriceType PriceType { get; set; }
    public decimal Amount { get; set; }
    public int? MinimumQuantity { get; set; } // relevant for Wholesale rows
}