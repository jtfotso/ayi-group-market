using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Notifications.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid NotificationId, string UserId) : IRequest;

public class MarkNotificationReadCommandHandler(IApplicationDbContext db) : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification is null) return; // silently ignore — not this user's notification, don't leak existence via an error

        notification.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
    }
}