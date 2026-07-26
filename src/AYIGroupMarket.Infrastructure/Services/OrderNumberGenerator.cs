using AYIGroupMarket.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Infrastructure.Services;

public class OrderNumberGenerator(Persistence.AppDbContext db) : IOrderNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"AYI-{year}-";

        var lastNumber = await db.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var nextSequence = 1;
        if (lastNumber is not null)
        {
            var sequencePart = lastNumber[prefix.Length..];
            if (int.TryParse(sequencePart, out var parsed))
                nextSequence = parsed + 1;
        }

        return $"{prefix}{nextSequence:D6}";
    }
}