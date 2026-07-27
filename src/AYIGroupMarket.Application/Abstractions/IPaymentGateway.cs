using AYIGroupMarket.Application.DTOs;

namespace AYIGroupMarket.Application.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentResultDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken);

    Task<PaymentVerificationResultDto> VerifyPaymentAsync(string transactionReference, CancellationToken cancellationToken);
}