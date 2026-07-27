using AYIGroupMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AYIGroupMarket.Infrastructure.Persistence.Configurations;

public class WholesaleAccountConfiguration : IEntityTypeConfiguration<WholesaleAccount>
{
    public void Configure(EntityTypeBuilder<WholesaleAccount> builder)
    {
        builder.HasIndex(w => w.UserId).IsUnique();
        builder.Property(w => w.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(w => w.ContactPerson).HasMaxLength(150).IsRequired();
        builder.Property(w => w.Phone).HasMaxLength(30).IsRequired();
        builder.Property(w => w.Email).HasMaxLength(200).IsRequired();
        builder.Property(w => w.BusinessAddress).HasMaxLength(300).IsRequired();
        builder.Property(w => w.City).HasMaxLength(100).IsRequired();
        builder.Property(w => w.ExpectedOrderVolume).HasMaxLength(100);
    }
}

public class QuoteRequestConfiguration : IEntityTypeConfiguration<QuoteRequest>
{
    public void Configure(EntityTypeBuilder<QuoteRequest> builder)
    {
        builder.Property(q => q.DeliveryLocation).HasMaxLength(300).IsRequired();
        builder.Property(q => q.QuotedTotal).HasColumnType("decimal(18,2)");

        builder.HasOne(q => q.WholesaleAccount)
            .WithMany()
            .HasForeignKey(q => q.WholesaleAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(q => q.Items)
            .WithOne(i => i.QuoteRequest)
            .HasForeignKey(i => i.QuoteRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuoteRequestItemConfiguration : IEntityTypeConfiguration<QuoteRequestItem>
{
    public void Configure(EntityTypeBuilder<QuoteRequestItem> builder)
    {
        builder.Property(i => i.QuotedUnitPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}