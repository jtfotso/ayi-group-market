namespace AYIGroupMarket.Application.DTOs;

public record PaymentRequestDto(
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string Currency,
    string CustomerPhone,
    string? CustomerEmail);

public record PaymentResultDto(
    bool Success,
    string? TransactionReference,
    string? RedirectUrl,   // for gateways that need a browser redirect (card, PayPal)
    string? ErrorMessage);

public record PaymentVerificationResultDto(
    bool IsVerified,
    string Status, // gateway-reported status string, e.g. "SUCCESSFUL", "FAILED", "PENDING"
    decimal? VerifiedAmount);