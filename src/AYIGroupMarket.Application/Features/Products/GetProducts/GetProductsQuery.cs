using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Products.GetProducts;

public record GetProductsQuery(
    Guid? CategoryId = null,
    string? SearchTerm = null,
    bool FeaturedOnly = false,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ProductListItemDto>>;

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 250);
    }
}

public class GetProductsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    public async Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive);

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.FeaturedOnly)
            query = query.Where(p => p.IsFeatured);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                p.NameEn.Contains(term) ||
                p.Sku.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListItemDto(
                p.Id, p.Sku, p.Slug, p.Name, p.NameEn,
                p.ShortDescription, p.ShortDescriptionEn,
                p.RetailPrice, p.IsFeatured,
                p.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault(),
                p.CategoryId, p.Category.Name, p.Category.NameEn))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
}