using AYIGroupMarket.Domain.Common;
using AYIGroupMarket.Domain.Enums;

namespace AYIGroupMarket.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // AYI-2026-000001

    public string OwnerKey { get; set; } = string.Empty; // ties back to the cart owner, same pattern as Cart

    public Guid AddressId { get; set; }
    public Address Address { get; set; } = null!;

    public Guid ShippingRateId { get; set; }
    public ShippingRate ShippingRate { get; set; } = null!;

    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }

    public string? Notes { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}