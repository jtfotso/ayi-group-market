namespace AYIGroupMarket.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}