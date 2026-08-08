using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Favorites.ToggleFavorite;

public record ToggleFavoriteCommand(string UserId, Guid ProductId) : IRequest<bool>; // returns new state (true = now favorited)

public class ToggleFavoriteCommandHandler(IApplicationDbContext db) : IRequestHandler<ToggleFavoriteCommand, bool>
{
    public async Task<bool> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
    {
        var existing = await db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.ProductId == request.ProductId, cancellationToken);

        if (existing is not null)
        {
            db.Favorites.Remove(existing);
            await db.SaveChangesAsync(cancellationToken);
            return false;
        }

        db.Favorites.Add(new Favorite { UserId = request.UserId, ProductId = request.ProductId });
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}