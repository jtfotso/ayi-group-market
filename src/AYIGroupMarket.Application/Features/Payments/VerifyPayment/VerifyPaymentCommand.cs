using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Payments.VerifyPayment;

public record VerifyPaymentCommand(string TransactionReference) : IRequest<bool>;

public class VerifyPaymentCommandHandler(IApplicationDbContext db, IPaymentGatewayResolver gatewayResolver)
    : IRequestHandler<VerifyPaymentCommand, bool>
{
    public async Task<bool> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await db.Payments
            .FirstOrDefaultAsync(p => p.TransactionReference == request.TransactionReference, cancellationToken)
            ?? throw new KeyNotFoundException("Payment not found for this transaction reference");

        // Already verified successfully — don't re-call the gateway or re-process, just confirm success
        if (payment.Status == PaymentStatus.Successful)
            return true;

        var gateway = gatewayResolver.Resolve(payment.Method);

        // This calls the gateway's OWN verification API — never trusts the webhook payload alone
        var verification = await gateway.VerifyPaymentAsync(request.TransactionReference, cancellationToken);

        db.PaymentTransactions.Add(new Domain.Entities.PaymentTransaction
        {
            PaymentId = payment.Id,
            EventType = "Verify",
            RawStatus = verification.Status,
            RawPayload = verification.Status // full detail preserved here, unbounded
        });

        if (verification.IsVerified)
        {
            payment.Status = PaymentStatus.Successful;

            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId, cancellationToken);
            if (order is not null)
                order.Status = OrderStatus.Paid;
        }

        await db.SaveChangesAsync(cancellationToken);
        return verification.IsVerified;
    }
}