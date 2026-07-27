using AYIGroupMarket.Domain.Common;
using AYIGroupMarket.Domain.Enums;

namespace AYIGroupMarket.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "XAF";

    public string? TransactionReference { get; set; } // our-side/gateway-issued reference
    public string? RedirectUrl { get; set; }

    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}