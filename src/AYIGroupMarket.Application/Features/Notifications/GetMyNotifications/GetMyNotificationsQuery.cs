using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Notifications.GetMyNotifications;

public record NotificationDto(Guid Id, string Title, string TitleEn, string Message, string MessageEn, string? LinkUrl, bool IsRead, DateTime CreatedAt);

public record GetMyNotificationsQuery(string UserId) : IRequest<List<NotificationDto>>;

public class GetMyNotificationsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await db.Notifications.AsNoTracking()
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new NotificationDto(n.Id, n.Title, n.TitleEn, n.Message, n.MessageEn, n.LinkUrl, n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}