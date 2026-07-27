using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    public string EventType { get; set; } = string.Empty; // "Initiate", "Verify", "Webhook"
    public string RawStatus { get; set; } = string.Empty; // whatever the gateway reported, verbatim
    public string? RawPayload { get; set; } // full webhook/response body, for debugging/audit
}