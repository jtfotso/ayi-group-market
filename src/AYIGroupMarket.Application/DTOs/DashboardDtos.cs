namespace AYIGroupMarket.Application.DTOs;

public record DashboardMetricsDto(
    decimal TotalRevenue,
    int TotalOrders,
    decimal AverageOrderValue,
    int TotalCustomers,
    int WholesaleCustomers,
    int LowStockProductCount,
    int PendingWholesaleApplications,
    int PendingOrders);