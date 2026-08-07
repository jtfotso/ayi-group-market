namespace AYIGroupMarket.Application.DTOs;

public record QuoteRequestItemInput(Guid ProductId, Guid? ProductVariantId, int Quantity);

public record QuoteRequestItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductNameEn,
    string? VariantName,
    string? VariantNameEn,
    int Quantity,
    decimal? QuotedUnitPrice);

public record QuoteRequestDto(
    Guid Id,
    string CompanyName,
    string DeliveryLocation,
    string? Message,
    string Status,
    decimal? QuotedTotal,
    DateTime CreatedAt,
    List<QuoteRequestItemDto> Items);