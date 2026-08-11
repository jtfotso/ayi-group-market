namespace AYIGroupMarket.Application.DTOs;

public record SalesPeriodDto(string PeriodLabel, DateTime PeriodStart, int OrderCount, decimal Revenue);

public record SalesReportDto(List<SalesPeriodDto> Periods, decimal TotalRevenue, int TotalOrders);