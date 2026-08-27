namespace AYIGroupMarket.Application.Abstractions;

public interface IInvoiceGenerator
{
    Task<byte[]> GenerateAsync(Guid orderId, bool isFrench, CancellationToken cancellationToken = default);
}