namespace AYIGroupMarket.Application.DTOs;

public record AdminOrderListItemDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string CustomerPhone,
    string Status,
    decimal Total,
    string PaymentMethod,
    DateTime CreatedAt);

public record AdminOrderDetailDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string PaymentMethod,
    string? TrackingNumber,
    string? Notes,
    DateTime? CustomerConfirmedAt,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    bool IsPickup,
    string? AddressLine,
    string? City,
    string? ShippingZoneName,
    List<OrderItemDto> Items,
    DateTime CreatedAt);