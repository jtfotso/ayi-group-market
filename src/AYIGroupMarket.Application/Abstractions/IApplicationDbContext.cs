using AYIGroupMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductPrice> ProductPrices { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}