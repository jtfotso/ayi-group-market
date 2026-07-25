using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Categories.GetCategories;

public record GetCategoriesQuery(bool IncludeInactive = false) : IRequest<List<ProductCategoryDto>>;

public class GetCategoriesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetCategoriesQuery, List<ProductCategoryDto>>
{
    public async Task<List<ProductCategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = db.ProductCategories.AsNoTracking().AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new ProductCategoryDto(c.Id, c.Name, c.NameEn, c.Slug, c.Icon, c.IsActive, c.DisplayOrder))
            .ToListAsync(cancellationToken);
    }
}