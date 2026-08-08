using AYIGroupMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AYIGroupMarket.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductPrice> ProductPrices { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<ShippingZone> ShippingZones { get; }
    DbSet<ShippingRate> ShippingRates { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<WholesaleAccount> WholesaleAccounts { get; }
    DbSet<ProductReview> ProductReviews { get; }
    DbSet<Favorite> Favorites { get; }
    DbSet<QuoteRequest> QuoteRequests { get; }
    DbSet<QuoteRequestItem> QuoteRequestItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}