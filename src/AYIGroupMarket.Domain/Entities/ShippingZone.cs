using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class ShippingZone : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g. "Littoral"
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ShippingRate> Rates { get; set; } = new List<ShippingRate>();
}