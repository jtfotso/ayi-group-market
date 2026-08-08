namespace AYIGroupMarket.Application.Abstractions;

public interface INotificationService
{
    Task NotifyAsync(string userId, string title, string titleEn, string message, string messageEn, string? linkUrl, CancellationToken cancellationToken = default);
}