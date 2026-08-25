using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Payments.GetPaymentReferenceByOrderNumber;

public record GetPaymentReferenceByOrderNumberQuery(string OrderNumber) : IRequest<string?>;

public class GetPaymentReferenceByOrderNumberQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPaymentReferenceByOrderNumberQuery, string?>
{
    public async Task<string?> Handle(GetPaymentReferenceByOrderNumberQuery request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == request.OrderNumber, cancellationToken);

        if (order is null) return null;

        var payment = await db.Payments.AsNoTracking()
            .Where(p => p.OrderId == order.Id)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return payment?.TransactionReference;
    }
}