using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.GetProductsForAdmin;

public record AdminProductListItemDto(
    Guid Id, string Sku, string Name, string CategoryName,
    decimal RetailPrice, decimal? WholesalePrice, int StockQuantity, bool IsActive);

public record GetProductsForAdminQuery(bool LowStockOnly = false, int LowStockThreshold = 10) : IRequest<List<AdminProductListItemDto>>;

public class GetProductsForAdminQueryHandler(IApplicationDbContext db)
        : IRequestHandler<GetProductsForAdminQuery, List<AdminProductListItemDto>>
    {
        public async Task<List<AdminProductListItemDto>> Handle(GetProductsForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

            if (request.LowStockOnly)
                query = query.Where(p => p.IsActive && p.StockQuantity <= request.LowStockThreshold);

            return await query
                .OrderBy(p => p.Name)
                .Select(p => new AdminProductListItemDto(
                    p.Id, p.Sku, p.Name, p.Category.Name,
                    p.RetailPrice, p.WholesalePrice, p.StockQuantity, p.IsActive))
                .ToListAsync(cancellationToken);
        }
    }