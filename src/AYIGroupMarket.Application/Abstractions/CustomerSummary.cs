namespace AYIGroupMarket.Application.Abstractions;

public record CustomerSummary(string Id, string Email, string FirstName, string LastName, DateTime CreatedAt);