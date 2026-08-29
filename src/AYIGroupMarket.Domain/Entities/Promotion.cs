using AYIGroupMarket.Domain.Common;
using AYIGroupMarket.Domain.Enums;

namespace AYIGroupMarket.Domain.Entities;

public class Promotion : BaseEntity
{
    public string Code { get; set; } = string.Empty; // e.g. "FRUILEG10"
    public PromotionType Type { get; set; }

    public decimal? DiscountPercentage { get; set; } // for PercentageDiscount
    public decimal? DiscountAmount { get; set; }      // for FixedAmount

    // Conditions
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? MinimumQuantity { get; set; }
    public PromotionCustomerType CustomerType { get; set; } = PromotionCustomerType.All;

    public Guid? ProductId { get; set; }   // for ProductSpecific
    public Product? Product { get; set; }
    public Guid? CategoryId { get; set; }  // for CategoryDiscount
    public ProductCategory? Category { get; set; }

    public string? TargetCustomerPhone { get; set; }
    public string? TargetCustomerEmail { get; set; }

    public bool IsActive { get; set; } = true;
    public int? MaxUses { get; set; }
    public int UsesCount { get; set; } = 0;
}