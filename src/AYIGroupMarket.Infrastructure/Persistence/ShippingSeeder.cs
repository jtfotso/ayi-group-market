using AYIGroupMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Infrastructure.Persistence;

public static class ShippingSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.ShippingZones.AnyAsync())
            return;

        var zoneNames = new[]
        {
            ("Littoral", "Littoral"),
            ("Centre", "Centre"),
            ("Ouest", "West"),
            ("Sud-Ouest", "South-West"),
            ("Nord-Ouest", "North-West"),
            ("Nord", "North"),
            ("Extrême-Nord", "Far North"),
            ("Adamaoua", "Adamawa"),
            ("Est", "East"),
            ("Sud", "South"),
        };

        // Littoral (Douala) and Centre (Yaoundé) get the fast/cheap tier — all other regions get the slower/costlier tier
        var majorCityZones = new HashSet<string> { "Littoral", "Centre" };

        foreach (var (nameFr, nameEn) in zoneNames)
        {
            var zone = new ShippingZone { Name = nameFr, NameEn = nameEn, IsActive = true };

            var isMajorCity = majorCityZones.Contains(nameFr);

            zone.Rates.Add(new ShippingRate
            {
                DeliveryMethod = "Livraison standard",
                DeliveryMethodEn = "Standard delivery",
                IsPickup = false,
                DeliveryDays = isMajorCity ? 3 : 14,
                BaseFee = isMajorCity ? 3500m : 10000m,
                IsActive = true
            });

            db.ShippingZones.Add(zone);
        }

        await db.SaveChangesAsync();

        // Global pickup option — not tied to any zone
        db.ShippingRates.Add(new ShippingRate
        {
            ShippingZoneId = null,
            DeliveryMethod = "Retrait en magasin",
            DeliveryMethodEn = "Store pickup",
            IsPickup = true,
            DeliveryDays = 0,
            BaseFee = 0m,
            IsActive = true
        });

        await db.SaveChangesAsync();
    }
}