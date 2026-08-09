using AYIGroupMarket.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace AYIGroupMarket.Infrastructure.Storage;

public class LocalFileStorageService(IConfiguration configuration) : IFileStorageService
{
    private const string UploadsSubfolder = "products";

    private string GetUploadsRootPath()
    {
        var configuredPath = configuration["FileStorage:LocalPath"];
        return !string.IsNullOrWhiteSpace(configuredPath)
            ? configuredPath
            : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "shared-uploads");
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var folderPath = Path.Combine(GetUploadsRootPath(), UploadsSubfolder);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, uniqueFileName);

        await using var output = File.Create(filePath);
        await fileStream.CopyToAsync(output, cancellationToken);

        return $"/uploads/products/{uniqueFileName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(GetUploadsRootPath(), UploadsSubfolder, fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}