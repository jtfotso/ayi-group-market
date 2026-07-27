using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Wholesale.GetMyWholesaleAccount;

public record GetMyWholesaleAccountQuery(string UserId) : IRequest<MyWholesaleAccountDto?>;

public record MyWholesaleAccountDto(Guid Id, string Status, string? RejectionReason);

public class GetMyWholesaleAccountQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMyWholesaleAccountQuery, MyWholesaleAccountDto?>
{
    public async Task<MyWholesaleAccountDto?> Handle(GetMyWholesaleAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await db.WholesaleAccounts.AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

        return account is null ? null : new MyWholesaleAccountDto(account.Id, account.Status.ToString(), account.RejectionReason);
    }
}