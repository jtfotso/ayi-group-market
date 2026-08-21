namespace AYIGroupMarket.Application.DTOs;

public record ShippingRateDto(
    Guid Id,
    string DeliveryMethod,
    string DeliveryMethodEn,
    bool IsPickup,
    int DeliveryDays,
    decimal BaseFee,
    decimal? FeePerKg,
    decimal? FreeShippingThreshold);

public record ShippingZoneDto(
    Guid Id,
    string Name,
    string NameEn,
    List<ShippingRateDto> Rates);

public record CreateAddressRequest(
    string FullName,
    string Phone,
    string Email,
    string? AddressLine,
    string? City,
    Guid? ShippingZoneId);

public record OrderItemDto(
    string ProductName,
    string ProductNameEn,
    string? VariantName,
    string? VariantNameEn,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    string Status,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    List<OrderItemDto> Items);