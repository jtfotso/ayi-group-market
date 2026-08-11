using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.GetSalesReport;

public enum ReportGrouping { Day, Week, Month }

public record GetSalesReportQuery(DateTime StartDate, DateTime EndDate, ReportGrouping Grouping) : IRequest<SalesReportDto>;

public class GetSalesReportQueryHandler(IApplicationDbContext db) : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    // Only orders that represent real, counted revenue — same definition as the dashboard metrics
    private static readonly OrderStatus[] RevenueStatuses =
        { OrderStatus.Paid, OrderStatus.Processing, OrderStatus.ReadyForDelivery, OrderStatus.Shipped, OrderStatus.Delivered };

    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        var orders = await db.Orders.AsNoTracking()
            .Where(o => RevenueStatuses.Contains(o.Status) && o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate)
            .Select(o => new { o.CreatedAt, o.Total })
            .ToListAsync(cancellationToken);

        var grouped = request.Grouping switch
        {
            ReportGrouping.Day => orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new SalesPeriodDto(g.Key.ToString("dd/MM"), g.Key, g.Count(), g.Sum(o => o.Total)))
                .OrderBy(p => p.PeriodStart),

            ReportGrouping.Week => orders
                .GroupBy(o => StartOfWeek(o.CreatedAt))
                .Select(g => new SalesPeriodDto($"Sem. {g.Key:dd/MM}", g.Key, g.Count(), g.Sum(o => o.Total)))
                .OrderBy(p => p.PeriodStart),

            ReportGrouping.Month => orders
                .GroupBy(o => new DateTime(o.CreatedAt.Year, o.CreatedAt.Month, 1))
                .Select(g => new SalesPeriodDto(g.Key.ToString("MMM yyyy"), g.Key, g.Count(), g.Sum(o => o.Total)))
                .OrderBy(p => p.PeriodStart),

            _ => throw new ArgumentOutOfRangeException(nameof(request.Grouping))
        };

        var periods = grouped.ToList();

        return new SalesReportDto(periods, periods.Sum(p => p.Revenue), periods.Sum(p => p.OrderCount));
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}