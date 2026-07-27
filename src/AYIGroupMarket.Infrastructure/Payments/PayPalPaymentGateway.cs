using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;

namespace AYIGroupMarket.Infrastructure.Payments;

/// <summary>
/// PLACEHOLDER — replace with the real PayPal Orders API (Create Order + Capture) once
/// PayPal REST API credentials (Client ID/Secret) are configured.
/// </summary>
public class PayPalPaymentGateway : IPaymentGateway
{
    public Task<PaymentResultDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        var reference = $"PAYPAL-{Guid.NewGuid():N}"[..20];
        return Task.FromResult(new PaymentResultDto(
            Success: true,
            TransactionReference: reference,
            RedirectUrl: "https://www.sandbox.paypal.com/checkoutnow", // placeholder — real flow returns PayPal's actual approval URL
            ErrorMessage: null));
    }

    public Task<PaymentVerificationResultDto> VerifyPaymentAsync(string transactionReference, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PaymentVerificationResultDto(IsVerified: false, Status: "PENDING", VerifiedAmount: null));
    }
}