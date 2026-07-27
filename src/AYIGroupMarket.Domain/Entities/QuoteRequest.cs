using AYIGroupMarket.Domain.Common;
using AYIGroupMarket.Domain.Enums;

namespace AYIGroupMarket.Domain.Entities;

public class QuoteRequest : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid WholesaleAccountId { get; set; }
    public WholesaleAccount WholesaleAccount { get; set; } = null!;

    public string DeliveryLocation { get; set; } = string.Empty;
    public string? Message { get; set; }

    public QuoteStatus Status { get; set; } = QuoteStatus.Submitted;
    public decimal? QuotedTotal { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public ICollection<QuoteRequestItem> Items { get; set; } = new List<QuoteRequestItem>();
}