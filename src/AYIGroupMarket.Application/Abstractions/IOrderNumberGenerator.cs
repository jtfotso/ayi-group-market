namespace AYIGroupMarket.Application.Abstractions;

public interface IOrderNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}