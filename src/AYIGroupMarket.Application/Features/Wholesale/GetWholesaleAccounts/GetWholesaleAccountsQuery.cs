using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Wholesale.GetWholesaleAccounts;

public record GetWholesaleAccountsQuery : IRequest<List<WholesaleAccount>>;

public class GetWholesaleAccountsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetWholesaleAccountsQuery, List<WholesaleAccount>>
{
    public async Task<List<WholesaleAccount>> Handle(GetWholesaleAccountsQuery request, CancellationToken cancellationToken)
    {
        return await db.WholesaleAccounts.AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}