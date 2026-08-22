using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
    public DbSet<ShippingRate> ShippingRates => Set<ShippingRate>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<WholesaleAccount> WholesaleAccounts => Set<WholesaleAccount>();
    public DbSet<QuoteRequestItem> QuoteRequestItems => Set<QuoteRequestItem>();
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // required — configures Identity's own tables

        // Apply all IEntityTypeConfiguration<T> classes in this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    public Task<int> GetTotalUserCountAsync(CancellationToken cancellationToken = default)
    => Users.CountAsync(cancellationToken);

    public async Task<List<Application.Abstractions.CustomerSummary>> GetCustomerSummariesAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(term)) ||
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term));
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new Application.Abstractions.CustomerSummary(u.Id, u.Email ?? "", u.FirstName, u.LastName, u.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<Application.Abstractions.CustomerSummary?> GetCustomerSummaryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user is null ? null : new Application.Abstractions.CustomerSummary(user.Id, user.Email ?? "", user.FirstName, user.LastName, user.CreatedAt);
    }

    public async Task<List<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await (from ur in UserRoles
                    join r in Roles on ur.RoleId equals r.Id
                    where ur.UserId == userId
                    select r.Name!)
                    .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetAllRoleNamesAsync(CancellationToken cancellationToken = default)
    {
        return await Roles.Select(r => r.Name!).ToListAsync(cancellationToken);
    }

}