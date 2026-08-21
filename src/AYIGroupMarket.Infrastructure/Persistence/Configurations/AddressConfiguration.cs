using AYIGroupMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AYIGroupMarket.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.Property(a => a.FullName).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Phone).HasMaxLength(30).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(200);
        builder.Property(a => a.AddressLine).HasMaxLength(300).IsRequired();
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();

        builder.HasOne(a => a.ShippingZone)
            .WithMany()
            .HasForeignKey(a => a.ShippingZoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}