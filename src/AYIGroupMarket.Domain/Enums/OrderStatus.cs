namespace AYIGroupMarket.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    PaymentPending = 1,
    Paid = 2,
    Processing = 3,
    ReadyForDelivery = 4,
    Shipped = 5,
    Delivered = 6,
    Cancelled = 7,
    Refunded = 8
}