namespace AYIGroupMarket.Application.DTOs;

public record AdminCustomerListItemDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    int OrderCount,
    decimal TotalSpent,
    bool IsWholesale,
    string? WholesaleStatus);

public record AdminCustomerDetailDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    List<OrderDto> Orders,
    string? WholesaleStatus,
    string? WholesaleCompanyName);