using System.Text;
using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Admin.ExportOrdersCsv;

public record ExportOrdersCsvQuery(DateTime StartDate, DateTime EndDate) : IRequest<byte[]>;

public class ExportOrdersCsvQueryHandler(IApplicationDbContext db) : IRequestHandler<ExportOrdersCsvQuery, byte[]>
{
    public async Task<byte[]> Handle(ExportOrdersCsvQuery request, CancellationToken cancellationToken)
    {
        var orders = await db.Orders.AsNoTracking()
            .Include(o => o.Address)
            .Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Order Number,Date,Customer,Phone,Status,Payment Method,Subtotal,Shipping,Discount,Total,Promo Code,Tracking Number");

        foreach (var o in orders)
        {
            sb.AppendLine(string.Join(",",
                CsvEscape(o.OrderNumber),
                o.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                CsvEscape(o.Address.FullName),
                CsvEscape(o.Address.Phone),
                o.Status.ToString(),
                o.PaymentMethod.ToString(),
                o.Subtotal.ToString("F2"),
                o.ShippingFee.ToString("F2"),
                o.DiscountAmount.ToString("F2"),
                o.Total.ToString("F2"),
                CsvEscape(o.PromoCode ?? ""),
                CsvEscape(o.TrackingNumber ?? "")));
        }

        // UTF-8 BOM so Excel correctly detects encoding and displays accented French characters properly
        var preamble = Encoding.UTF8.GetPreamble();
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return preamble.Concat(csvBytes).ToArray();
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}