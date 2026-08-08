using AYIGroupMarket.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace AYIGroupMarket.Api.Endpoints;

public static class WholesaleCatalogEndpoint
{
    public static void MapWholesaleCatalogEndpoint(this WebApplication app)
    {
        app.MapGet("/api/wholesale/catalog", async (
            IWholesaleCatalogGenerator generator, bool fr, CancellationToken cancellationToken) =>
        {
            var pdfBytes = await generator.GenerateAsync(fr, cancellationToken);
            return Results.File(pdfBytes, "application/pdf", "AYI-Group-Market-Catalogue-Grossiste.pdf");
        })
        .RequireAuthorization("WholesaleCustomer");
    }
}