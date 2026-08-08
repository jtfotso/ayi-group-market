using AYIGroupMarket.Domain.Common;
using AYIGroupMarket.Domain.Enums;

namespace AYIGroupMarket.Domain.Entities;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty; // snapshot, in case the user later changes their name

    public int Rating { get; set; } // 1-5
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;

    public bool IsVerifiedPurchase { get; set; } // "Achat vérifié" — only true if tied to a real completed order
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
}