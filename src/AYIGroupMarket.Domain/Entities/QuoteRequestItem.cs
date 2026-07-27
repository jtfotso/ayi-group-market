using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class QuoteRequestItem : BaseEntity
{
    public Guid QuoteRequestId { get; set; }
    public QuoteRequest QuoteRequest { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public int Quantity { get; set; }
    public decimal? QuotedUnitPrice { get; set; } // filled in once admin responds
}