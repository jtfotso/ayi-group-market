using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class Cart : BaseEntity
{
    public string OwnerKey { get; set; } = string.Empty; // "user:{userId}" or "session:{guid}"

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}