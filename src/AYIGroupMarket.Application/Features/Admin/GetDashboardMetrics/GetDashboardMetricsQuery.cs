using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.GetDashboardMetrics;

public record GetDashboardMetricsQuery(int LowStockThreshold = 10) : IRequest<DashboardMetricsDto>;

public class GetDashboardMetricsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetDashboardMetricsQuery, DashboardMetricsDto>
{
    public async Task<DashboardMetricsDto> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        // "Revenue" counted from Paid+ orders only — Pending/PaymentPending orders aren't real revenue yet
        var paidStatuses = new[] { OrderStatus.Paid, OrderStatus.Processing, OrderStatus.ReadyForDelivery, OrderStatus.Shipped, OrderStatus.Delivered };

        var paidOrders = await db.Orders.AsNoTracking()
            .Where(o => paidStatuses.Contains(o.Status))
            .ToListAsync(cancellationToken);

        var totalRevenue = paidOrders.Sum(o => o.Total);
        var totalOrders = paidOrders.Count;
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var totalCustomers = await db.GetTotalUserCountAsync(cancellationToken);

        // Wholesale customers = approved accounts, not just anyone who registered
        var wholesaleCustomers = await db.WholesaleAccounts
            .CountAsync(w => w.Status == WholesaleStatus.Approved, cancellationToken);

        var lowStockProductCount = await db.Products
            .CountAsync(p => p.IsActive && p.StockQuantity <= request.LowStockThreshold, cancellationToken);

        var pendingWholesaleApplications = await db.WholesaleAccounts
            .CountAsync(w => w.Status == WholesaleStatus.Pending, cancellationToken);

        var pendingOrders = await db.Orders
            .CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PaymentPending, cancellationToken);

        return new DashboardMetricsDto(
            totalRevenue, totalOrders, averageOrderValue, totalCustomers,
            wholesaleCustomers, lowStockProductCount, pendingWholesaleApplications, pendingOrders);
    }
}