using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;

namespace AYIGroupMarket.Infrastructure.Payments;

/// <summary>
/// No real payment processing — order is confirmed manually via WhatsApp, per spec section 19.
/// Always "succeeds" immediately since there's no gateway to verify against.
/// </summary>
public class WhatsAppManualPaymentGateway : IPaymentGateway
{
    public Task<PaymentResultDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        var reference = $"WHATSAPP-{request.OrderNumber}";
        return Task.FromResult(new PaymentResultDto(true, reference, null, null));
    }

    public Task<PaymentVerificationResultDto> VerifyPaymentAsync(string transactionReference, CancellationToken cancellationToken)
    {
        // Manual orders are marked paid by an admin, not verified automatically
        return Task.FromResult(new PaymentVerificationResultDto(IsVerified: false, Status: "AWAITING_MANUAL_CONFIRMATION", VerifiedAmount: null));
    }
}