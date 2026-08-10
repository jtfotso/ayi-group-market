namespace AYIGroupMarket.Application.DTOs;

public record ValidatePromotionResultDto(bool IsValid, string? ErrorMessage, decimal DiscountAmount);