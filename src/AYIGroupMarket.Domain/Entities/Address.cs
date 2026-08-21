using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class Address : BaseEntity
{
    public string? UserId { get; set; } // null for guest checkout addresses

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public Guid? ShippingZoneId { get; set; } // region
    public ShippingZone? ShippingZone { get; set; } = null!;
}