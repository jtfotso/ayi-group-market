using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;

namespace AYIGroupMarket.Infrastructure.Payments;

/// <summary>
/// PLACEHOLDER — replace with the real Orange Money Web Payment API once merchant
/// credentials are available. Currently simulates an immediate "pending" response.
/// </summary>
public class OrangeMoneyPaymentGateway : IPaymentGateway
{
    public Task<PaymentResultDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        var reference = $"OM-{Guid.NewGuid():N}"[..20];
        return Task.FromResult(new PaymentResultDto(
            Success: true,
            TransactionReference: reference,
            RedirectUrl: null, // Orange Money typically pushes a USSD/app confirmation, no redirect needed
            ErrorMessage: null));
    }

    public Task<PaymentVerificationResultDto> VerifyPaymentAsync(string transactionReference, CancellationToken cancellationToken)
    {
        // PLACEHOLDER: real implementation calls Orange Money's status API using transactionReference
        return Task.FromResult(new PaymentVerificationResultDto(IsVerified: false, Status: "PENDING", VerifiedAmount: null));
    }
}