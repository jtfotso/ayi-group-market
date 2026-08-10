using AYIGroupMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AYIGroupMarket.Infrastructure.Persistence.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();

        builder.Property(p => p.DiscountPercentage).HasColumnType("decimal(5,2)");
        builder.Property(p => p.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.MinimumOrderAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.Product)
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}