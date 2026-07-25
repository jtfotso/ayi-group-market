using AYIGroupMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AYIGroupMarket.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasIndex(v => v.Sku).IsUnique();
        builder.Property(v => v.Sku).HasMaxLength(50).IsRequired();
        builder.Property(v => v.Name).HasMaxLength(100).IsRequired();

        builder.HasMany(v => v.Prices)
            .WithOne(p => p.ProductVariant)
            .HasForeignKey(p => p.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}