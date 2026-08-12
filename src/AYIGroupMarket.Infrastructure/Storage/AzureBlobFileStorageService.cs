using Azure.Storage.Blobs;
using AYIGroupMarket.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace AYIGroupMarket.Infrastructure.Storage;

public class AzureBlobFileStorageService(IConfiguration configuration) : IFileStorageService
{
    private const string ContainerName = "product-images";

    private BlobContainerClient GetContainerClient()
    {
        var connectionString = configuration["AppBlobStorage:ConnectionString"]
            ?? throw new InvalidOperationException("AppBlobStorage:ConnectionString is not configured.");

        var serviceClient = new BlobServiceClient(connectionString);
        return serviceClient.GetBlobContainerClient(ContainerName);
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var containerClient = GetContainerClient();
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        await blobClient.UploadAsync(fileStream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
        {
            ContentType = contentType
        }, cancellationToken: cancellationToken);

        Console.WriteLine($"[BlobStorage] Uploaded to: {blobClient.Uri}");

        return blobClient.Uri.ToString(); // full public URL, works identically from Web and Admin
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var containerClient = GetContainerClient();
        var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}