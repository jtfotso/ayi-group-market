using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;

namespace AYIGroupMarket.Infrastructure.Payments;

/// <summary>
/// PLACEHOLDER — replace with a real card processor (e.g. Stripe, Flutterwave) once
/// credentials are available. Card payments typically need a hosted checkout redirect,
/// so RedirectUrl is populated here (unlike Mobile Money/Orange Money which use USSD prompts).
/// </summary>
public class CardPaymentGateway : IPaymentGateway
{
    public Task<PaymentResultDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        var reference = $"CARD-{Guid.NewGuid():N}"[..20];
        return Task.FromResult(new PaymentResultDto(
            Success: true,
            TransactionReference: reference,
            RedirectUrl: "https://checkout.example.com/placeholder", // real gateway returns its actual hosted checkout URL
            ErrorMessage: null));
    }

    public Task<PaymentVerificationResultDto> VerifyPaymentAsync(string transactionReference, CancellationToken cancellationToken)
    {
        // PLACEHOLDER: real implementation calls the card processor's status/webhook verification API
        return Task.FromResult(new PaymentVerificationResultDto(IsVerified: false, Status: "PENDING", VerifiedAmount: null));
    }
}