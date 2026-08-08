using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Favorites.GetMyFavorites;

public record GetMyFavoritesQuery(string UserId) : IRequest<List<FavoriteProductDto>>;

public class GetMyFavoritesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMyFavoritesQuery, List<FavoriteProductDto>>
{
    public async Task<List<FavoriteProductDto>> Handle(GetMyFavoritesQuery request, CancellationToken cancellationToken)
    {
        return await db.Favorites.AsNoTracking()
            .Where(f => f.UserId == request.UserId)
            .Include(f => f.Product).ThenInclude(p => p.Images)
            .Select(f => new FavoriteProductDto(
                f.Product.Id, f.Product.Name, f.Product.NameEn, f.Product.Slug, f.Product.RetailPrice,
                f.Product.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault()))
            .ToListAsync(cancellationToken);
    }
}