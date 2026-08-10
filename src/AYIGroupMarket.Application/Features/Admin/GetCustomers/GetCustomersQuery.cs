using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.GetCustomers;

public record GetCustomersQuery(string? SearchTerm = null) : IRequest<List<AdminCustomerListItemDto>>;

public class GetCustomersQueryHandler(IApplicationDbContext db) : IRequestHandler<GetCustomersQuery, List<AdminCustomerListItemDto>>
{
    public async Task<List<AdminCustomerListItemDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await db.GetCustomerSummariesAsync(request.SearchTerm, cancellationToken);

        var result = new List<AdminCustomerListItemDto>();

        foreach (var customer in customers)
        {
            var ownerKey = $"user:{customer.Id}";

            var orders = await db.Orders.AsNoTracking()
                .Where(o => o.OwnerKey == ownerKey)
                .ToListAsync(cancellationToken);

            var wholesaleAccount = await db.WholesaleAccounts.AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == customer.Id, cancellationToken);

            result.Add(new AdminCustomerListItemDto(
                customer.Id, customer.Email, customer.FirstName, customer.LastName, customer.CreatedAt,
                orders.Count, orders.Sum(o => o.Total),
                wholesaleAccount is not null, wholesaleAccount?.Status.ToString()));
        }

        return result;
    }
}