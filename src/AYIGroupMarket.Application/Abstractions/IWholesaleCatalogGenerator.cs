namespace AYIGroupMarket.Application.Abstractions;

public interface IWholesaleCatalogGenerator
{
    Task<byte[]> GenerateAsync(bool isFrench, CancellationToken cancellationToken = default);
}