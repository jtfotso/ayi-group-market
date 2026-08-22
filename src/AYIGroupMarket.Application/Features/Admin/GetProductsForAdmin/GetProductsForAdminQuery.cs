using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.GetProductsForAdmin;

public record AdminProductListItemDto(
    Guid Id, string Sku, string Name, string CategoryName,
    decimal RetailPrice, decimal? WholesalePrice, int StockQuantity, bool IsActive);

public record GetProductsForAdminQuery : IRequest<List<AdminProductListItemDto>>;

public class GetProductsForAdminQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductsForAdminQuery, List<AdminProductListItemDto>>
{
    public async Task<List<AdminProductListItemDto>> Handle(GetProductsForAdminQuery request, CancellationToken cancellationToken)
    {
        return await db.Products.AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .Select(p => new AdminProductListItemDto(
                p.Id, p.Sku, p.Name, p.Category.Name,
                p.RetailPrice, p.WholesalePrice, p.StockQuantity, p.IsActive))
            .ToListAsync(cancellationToken);
    }
}