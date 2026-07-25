namespace AYIGroupMarket.Application.DTOs;

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductNameEn,
    string ProductSlug,
    Guid? VariantId,
    string? VariantName,
    string? VariantNameEn,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record CartDto(
    Guid Id,
    List<CartItemDto> Items,
    decimal Total,
    int ItemCount);