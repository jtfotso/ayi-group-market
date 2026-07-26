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

        foreach (var (nameFr, nameEn) in zoneNames)
        {
            var zone = new ShippingZone { Name = nameFr, NameEn = nameEn, IsActive = true };

            zone.Rates.Add(new ShippingRate
            {
                DeliveryMethod = "Standard",
                DeliveryMethodEn = "Standard",
                BaseFee = 0, // to be set via Admin Dashboard, per spec section 20
                IsActive = true
            });

            db.ShippingZones.Add(zone);
        }

        await db.SaveChangesAsync();
    }
}