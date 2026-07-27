using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;

namespace AYIGroupMarket.Infrastructure.Payments;

/// <summary>
/// PLACEHOLDER — replace with a real MTN/Orange Mobile Money aggregator (e.g. Campay, MTN MoMo API)
/// once credentials are available. Currently simulates an immediate "pending" response.
/// </summary>
public class MobileMoneyPaymentGateway : IPaymentGateway
{
    public Task<PaymentResultDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        var reference = $"MOMO-{Guid.NewGuid():N}"[..20];
        return Task.FromResult(new PaymentResultDto(
            Success: true,
            TransactionReference: reference,
            RedirectUrl: null, // Mobile Money typically pushes a USSD prompt, no redirect needed
            ErrorMessage: null));
    }

    public Task<PaymentVerificationResultDto> VerifyPaymentAsync(string transactionReference, CancellationToken cancellationToken)
    {
        // PLACEHOLDER: real implementation calls the provider's status API using transactionReference
        return Task.FromResult(new PaymentVerificationResultDto(IsVerified: false, Status: "PENDING", VerifiedAmount: null));
    }
}