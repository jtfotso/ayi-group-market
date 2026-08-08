using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Favorites.IsFavorite;

public record IsFavoriteQuery(string UserId, Guid ProductId) : IRequest<bool>;

public class IsFavoriteQueryHandler(IApplicationDbContext db) : IRequestHandler<IsFavoriteQuery, bool>
{
    public async Task<bool> Handle(IsFavoriteQuery request, CancellationToken cancellationToken)
    {
        return await db.Favorites.AsNoTracking()
            .AnyAsync(f => f.UserId == request.UserId && f.ProductId == request.ProductId, cancellationToken);
    }
}