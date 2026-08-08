using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Infrastructure.Persistence;

namespace AYIGroupMarket.Infrastructure.Services;

public class NotificationService(AppDbContext db) : INotificationService
{
    public async Task NotifyAsync(string userId, string title, string titleEn, string message, string messageEn, string? linkUrl, CancellationToken cancellationToken = default)
    {
        db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            TitleEn = titleEn,
            Message = message,
            MessageEn = messageEn,
            LinkUrl = linkUrl
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}