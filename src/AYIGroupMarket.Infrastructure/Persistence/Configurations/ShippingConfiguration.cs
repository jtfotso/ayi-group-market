using AYIGroupMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AYIGroupMarket.Infrastructure.Persistence.Configurations;

public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
{
    public void Configure(EntityTypeBuilder<ShippingZone> builder)
    {
        builder.Property(z => z.Name).HasMaxLength(100).IsRequired();
        builder.Property(z => z.NameEn).HasMaxLength(100);

        builder.HasMany(z => z.Rates)
            .WithOne(r => r.ShippingZone)
            .HasForeignKey(r => r.ShippingZoneId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}

public class ShippingRateConfiguration : IEntityTypeConfiguration<ShippingRate>
{
    public void Configure(EntityTypeBuilder<ShippingRate> builder)
    {
        builder.Property(r => r.DeliveryMethod).HasMaxLength(100).IsRequired();
        builder.Property(r => r.BaseFee).HasColumnType("decimal(18,2)");
        builder.Property(r => r.FeePerKg).HasColumnType("decimal(18,2)");
        builder.Property(r => r.FreeShippingThreshold).HasColumnType("decimal(18,2)");
    }
}