using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Payments.InitiatePayment;

public record InitiatePaymentCommand(Guid OrderId) : IRequest<InitiatePaymentResult>;

public record InitiatePaymentResult(bool Success, string? RedirectUrl, string? ErrorMessage);

public class InitiatePaymentCommandHandler(IApplicationDbContext db, IPaymentGatewayResolver gatewayResolver)
    : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResult>
{
    public async Task<InitiatePaymentResult> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found");

        var gateway = gatewayResolver.Resolve(order.PaymentMethod);

        var paymentRequest = new DTOs.PaymentRequestDto(
            order.Id, order.OrderNumber, order.Total, "XAF",
            CustomerPhone: "", // populated from Address in a follow-up if the gateway needs it directly
            CustomerEmail: null);

        var result = await gateway.InitiatePaymentAsync(paymentRequest, cancellationToken);

        var payment = new Payment
        {
            OrderId = order.Id,
            Method = order.PaymentMethod,
            Status = result.Success ? PaymentStatus.Pending : PaymentStatus.Failed,
            Amount = order.Total,
            TransactionReference = result.TransactionReference,
            RedirectUrl = result.RedirectUrl
        };
        db.Payments.Add(payment);

        var transaction = new PaymentTransaction
        {
            PaymentId = payment.Id,
            EventType = "Initiate",
            RawStatus = result.Success ? "INITIATED" : "FAILED",
            RawPayload = result.ErrorMessage
        };
        db.PaymentTransactions.Add(transaction);

        await db.SaveChangesAsync(cancellationToken);

        return new InitiatePaymentResult(result.Success, result.RedirectUrl, result.ErrorMessage);
    }
}