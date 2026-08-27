using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AYIGroupMarket.Infrastructure.Pdf;

public class InvoiceGenerator(AppDbContext db) : IInvoiceGenerator
{
    public async Task<byte[]> GenerateAsync(Guid orderId, bool isFrench, CancellationToken cancellationToken = default)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.Address).ThenInclude(a => a!.ShippingZone)
            .Include(o => o.ShippingRate)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("AYI GROUP MARKET").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                            c.Item().Text("Zone MAGZI Douala-Bassa, BP 7789").FontSize(9);
                            c.Item().Text("Douala, Cameroun").FontSize(9);
                            c.Item().Text("WhatsApp: +237 695 16 20 19").FontSize(9);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text(isFrench ? "FACTURE" : "INVOICE").FontSize(20).Bold();
                            c.Item().Text($"{(isFrench ? "N°" : "No.")} {order.OrderNumber}").FontSize(11);
                            c.Item().Text(order.CreatedAt.ToString("dd/MM/yyyy")).FontSize(9);
                        });
                    });
                    col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(isFrench ? "Facturé à" : "Billed to").Bold().FontSize(10);
                            c.Item().Text(order.CustomerName).FontSize(10);
                            c.Item().Text(order.CustomerPhone).FontSize(10);
                            if (!string.IsNullOrEmpty(order.CustomerEmail))
                                c.Item().Text(order.CustomerEmail).FontSize(10);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(isFrench ? "Livraison" : "Delivery").Bold().FontSize(10);
                            if (order.IsPickup)
                            {
                                c.Item().Text(isFrench ? "Retrait en magasin" : "Store pickup").FontSize(10);
                            }
                            else if (order.Address is not null)
                            {
                                c.Item().Text(order.Address.AddressLine).FontSize(10);
                                c.Item().Text($"{order.Address.City}, {order.Address.ShippingZone?.Name}").FontSize(10);
                            }
                            c.Item().Text($"{(isFrench ? "Paiement" : "Payment")}: {order.PaymentMethod}").FontSize(10);
                            c.Item().Text($"{(isFrench ? "Statut" : "Status")}: {order.Status}").FontSize(10);
                        });
                    });

                    col.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text(isFrench ? "Produit" : "Product").Bold();
                            header.Cell().AlignCenter().Text(isFrench ? "Qté" : "Qty").Bold();
                            header.Cell().AlignRight().Text(isFrench ? "Prix unitaire" : "Unit price").Bold();
                            header.Cell().AlignRight().Text(isFrench ? "Total" : "Total").Bold();
                        });

                        foreach (var item in order.Items)
                        {
                            var name = isFrench ? item.ProductNameSnapshot : item.ProductNameEnSnapshot;
                            var variant = isFrench ? item.VariantNameSnapshot : item.VariantNameEnSnapshot;
                            var displayName = variant is not null ? $"{name} — {variant}" : name;

                            table.Cell().Text(displayName);
                            table.Cell().AlignCenter().Text(item.Quantity.ToString());
                            table.Cell().AlignRight().Text(item.UnitPrice.ToString("C"));
                            table.Cell().AlignRight().Text(item.LineTotal.ToString("C"));
                        }
                    });

                    col.Item().PaddingTop(15).AlignRight().Column(c =>
                    {
                        c.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text(isFrench ? "Sous-total" : "Subtotal");
                            row.ConstantItem(100).AlignRight().Text(order.Subtotal.ToString("C"));
                        });
                        c.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text(isFrench ? "Livraison" : "Shipping");
                            row.ConstantItem(100).AlignRight().Text(order.ShippingFee.ToString("C"));
                        });
                        if (order.DiscountAmount > 0)
                        {
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().AlignRight().Text(isFrench ? "Réduction" : "Discount");
                                row.ConstantItem(100).AlignRight().Text($"-{order.DiscountAmount:C}");
                            });
                        }
                        c.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        c.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text(isFrench ? "Total" : "Total").Bold().FontSize(12);
                            row.ConstantItem(100).AlignRight().Text(order.Total.ToString("C")).Bold().FontSize(12);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span(isFrench
                        ? "Merci pour votre confiance — AYI GROUP MARKET"
                        : "Thank you for your business — AYI GROUP MARKET")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }
}