using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class ShippingRate : BaseEntity
{
    public Guid? ShippingZoneId { get; set; }
    public ShippingZone? ShippingZone { get; set; } = null!;

    public string DeliveryMethod { get; set; } = string.Empty; // e.g. "Standard", "Express"
    public string DeliveryMethodEn { get; set; } = string.Empty;

    public bool IsPickup { get; set; } = false; // true = "Retrait en magasin", no zone/address needed
    public int DeliveryDays { get; set; }

    public decimal BaseFee { get; set; }
    public decimal? FeePerKg { get; set; }          // additional cost by order weight
    public decimal? FreeShippingThreshold { get; set; } // order value above which shipping is free

    public bool IsActive { get; set; } = true;
}