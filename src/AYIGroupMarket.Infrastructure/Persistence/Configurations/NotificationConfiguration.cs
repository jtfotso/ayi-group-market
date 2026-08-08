using AYIGroupMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AYIGroupMarket.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.TitleEn).HasMaxLength(200);
        builder.Property(n => n.Message).HasMaxLength(500).IsRequired();
        builder.Property(n => n.MessageEn).HasMaxLength(500);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}