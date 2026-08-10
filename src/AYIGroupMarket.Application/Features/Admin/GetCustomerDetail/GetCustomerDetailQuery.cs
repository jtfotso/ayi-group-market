using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.GetCustomerDetail;

public record GetCustomerDetailQuery(string UserId) : IRequest<AdminCustomerDetailDto?>;

public class GetCustomerDetailQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetCustomerDetailQuery, AdminCustomerDetailDto?>
{
    public async Task<AdminCustomerDetailDto?> Handle(GetCustomerDetailQuery request, CancellationToken cancellationToken)
    {
        var customer = await db.GetCustomerSummaryAsync(request.UserId, cancellationToken);
        if (customer is null) return null;

        var ownerKey = $"user:{request.UserId}";

        var orders = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.OwnerKey == ownerKey)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var orderDtos = orders.Select(o => new OrderDto(
            o.Id, o.OrderNumber, o.Status.ToString(), o.Subtotal, o.ShippingFee, o.Total,
            o.Items.Select(i => new OrderItemDto(
                i.ProductNameSnapshot, i.ProductNameEnSnapshot,
                i.VariantNameSnapshot, i.VariantNameEnSnapshot,
                i.UnitPrice, i.Quantity, i.LineTotal
            )).ToList()
        )).ToList();

        var wholesaleAccount = await db.WholesaleAccounts.AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

        return new AdminCustomerDetailDto(
            customer.Id, customer.Email, customer.FirstName, customer.LastName, customer.CreatedAt,
            orderDtos, wholesaleAccount?.Status.ToString(), wholesaleAccount?.CompanyName);
    }
}