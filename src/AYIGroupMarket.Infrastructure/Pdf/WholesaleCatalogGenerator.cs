using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using AYIGroupMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AYIGroupMarket.Infrastructure.Pdf;

public class WholesaleCatalogGenerator(AppDbContext db) : IWholesaleCatalogGenerator
{
    public async Task<byte[]> GenerateAsync(bool isFrench, CancellationToken cancellationToken = default)
    {
        var categories = await db.ProductCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Include(c => c.Products.Where(p => p.IsActive))
                .ThenInclude(p => p.Variants)
                    .ThenInclude(v => v.Prices)
            .ToListAsync(cancellationToken);

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(isFrench ? "Catalogue Grossiste" : "Wholesale Catalog")
                        .FontSize(20).Bold();
                    col.Item().Text("AYI GROUP MARKET").FontSize(12).FontColor(Colors.Green.Darken2);
                    col.Item().Text(DateTime.UtcNow.ToString("dd/MM/yyyy")).FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    foreach (var category in categories)
                    {
                        if (!category.Products.Any()) continue;

                        col.Item().PaddingTop(15).Text(isFrench ? category.Name : category.NameEn)
                            .FontSize(14).Bold().FontColor(Colors.Green.Darken2);

                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text(isFrench ? "Produit" : "Product").Bold();
                                header.Cell().Text(isFrench ? "Format" : "Size").Bold();
                                header.Cell().Text(isFrench ? "Prix gros" : "Wholesale price").Bold();
                                header.Cell().Text(isFrench ? "Qté min." : "Min. qty").Bold();
                            });

                            foreach (var product in category.Products)
                            {
                                var name = isFrench ? product.Name : product.NameEn;

                                if (product.Variants.Any())
                                {
                                    foreach (var variant in product.Variants)
                                    {
                                        var wholesalePrice = variant.Prices.FirstOrDefault(p => p.PriceType == PriceType.Wholesale);
                                        if (wholesalePrice is null) continue;

                                        table.Cell().Text(name);
                                        table.Cell().Text(isFrench ? variant.Name : variant.NameEn);
                                        table.Cell().Text($"{wholesalePrice.Amount:N0} FCFA");
                                        table.Cell().Text(wholesalePrice.MinimumQuantity?.ToString() ?? "-");
                                    }
                                }
                                else if (product.WholesalePrice.HasValue)
                                {
                                    table.Cell().Text(name);
                                    table.Cell().Text("-");
                                    table.Cell().Text($"{product.WholesalePrice.Value:N0} FCFA");
                                    table.Cell().Text(product.MinimumWholesaleQuantity?.ToString() ?? "-");
                                }
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span(isFrench
                        ? "Prix réservés aux comptes grossistes approuvés — sujets à modification."
                        : "Prices reserved for approved wholesale accounts — subject to change.")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }
}