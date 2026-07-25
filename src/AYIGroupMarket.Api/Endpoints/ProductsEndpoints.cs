using AYIGroupMarket.Application.Features.Products.GetProductBySlug;
using AYIGroupMarket.Application.Features.Products.GetProducts;
using MediatR;

namespace AYIGroupMarket.Api.Endpoints;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (
            ISender sender,
            Guid? categoryId,
            string? search,
            bool featuredOnly = false,
            int page = 1,
            int pageSize = 20) =>
        {
            var query = new GetProductsQuery(categoryId, search, featuredOnly, page, pageSize);
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetProducts")
        .Produces<PagedResult<AYIGroupMarket.Application.DTOs.ProductListItemDto>>(StatusCodes.Status200OK);

        group.MapGet("/{slug}", async (ISender sender, string slug) =>
        {
            var result = await sender.Send(new GetProductBySlugQuery(slug));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetProductBySlug")
        .Produces<AYIGroupMarket.Application.DTOs.ProductDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}