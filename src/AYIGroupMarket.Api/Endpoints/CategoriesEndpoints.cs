using AYIGroupMarket.Application.Features.Categories.GetCategories;
using MediatR;

namespace AYIGroupMarket.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static void MapCategoriesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (ISender sender, bool includeInactive = false) =>
        {
            var result = await sender.Send(new GetCategoriesQuery(includeInactive));
            return Results.Ok(result);
        })
        .WithName("GetCategories")
        .Produces<List<AYIGroupMarket.Application.DTOs.ProductCategoryDto>>(StatusCodes.Status200OK);
    }
}