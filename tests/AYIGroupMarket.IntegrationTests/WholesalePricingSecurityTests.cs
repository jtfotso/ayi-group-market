using AYIGroupMarket.Application.Features.Products.GetProductPricing;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AYIGroupMarket.IntegrationTests;

public class WholesalePricingSecurityTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test, avoids cross-test pollution
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<Guid> SeedProductWithWholesalePricing(AppDbContext db)
    {
        var category = new ProductCategory
        {
            Name = "Test Category",
            NameEn = "Test Category",
            Slug = "test-category",
            Icon = "🧃",
            IsActive = true
        };
        db.ProductCategories.Add(category);

        var product = new Product
        {
            CategoryId = category.Id,
            Category = category,
            Sku = "TEST-SKU",
            Slug = "test-product",
            Name = "Produit Test",
            NameEn = "Test Product",
            RetailPrice = 1500m,
            WholesalePrice = 900m,           // this must NEVER leak to a non-wholesale caller
            MinimumWholesaleQuantity = 12,
            IsActive = true
        };
        db.Products.Add(product);

        await db.SaveChangesAsync();
        return product.Id;
    }

    [Fact]
    public async Task GetProductPricingQuery_WhenNotWholesaleAuthorized_NeverReturnsWholesalePrice()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var productId = await SeedProductWithWholesalePricing(db);
        var handler = new GetProductPricingQueryHandler(db);

        // Act — simulate an unauthenticated/retail caller
        var result = await handler.Handle(
            new GetProductPricingQuery(productId, IsWholesaleAuthorized: false),
            CancellationToken.None);

        // Assert — the security-critical guarantee from spec section 26
        result.WholesalePrice.Should().BeNull(
            "wholesale pricing must never be exposed to a caller that isn't a verified wholesale account");
        result.MinimumWholesaleQuantity.Should().BeNull(
            "minimum wholesale quantity is wholesale-only information and must not leak either");
        result.RetailPrice.Should().Be(1500m, "retail price should still be visible to everyone");
    }

    [Fact]
    public async Task GetProductPricingQuery_WhenWholesaleAuthorized_ReturnsWholesalePrice()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var productId = await SeedProductWithWholesalePricing(db);
        var handler = new GetProductPricingQueryHandler(db);

        // Act — simulate a verified, approved wholesale caller
        var result = await handler.Handle(
            new GetProductPricingQuery(productId, IsWholesaleAuthorized: true),
            CancellationToken.None);

        // Assert
        result.WholesalePrice.Should().Be(900m);
        result.MinimumWholesaleQuantity.Should().Be(12);
    }

    [Fact]
    public async Task AddToCartCommand_WhenWholesaleQuantityBelowMinimum_ThrowsAndRejectsOrder()
    {
        // Arrange — matches spec section 27's explicit example:
        // "MinimumWholesaleQuantity = 5 cartons. A wholesale order with 3 cartons must be rejected."
        await using var db = CreateInMemoryContext();

        var category = new ProductCategory { Name = "Cat", NameEn = "Cat", Slug = "cat", Icon = "🧃", IsActive = true };
        db.ProductCategories.Add(category);

        var product = new Product
        {
            CategoryId = category.Id,
            Category = category,
            Sku = "CARTON-SKU",
            Slug = "carton-product",
            Name = "Carton Test",
            NameEn = "Carton Test",
            RetailPrice = 15000m,
            WholesalePrice = 12000m,
            MinimumWholesaleQuantity = 5,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var handler = new AYIGroupMarket.Application.Features.Cart.AddToCart.AddToCartCommandHandler(db);

        // Act
        var act = async () => await handler.Handle(
            new AYIGroupMarket.Application.Features.Cart.AddToCart.AddToCartCommand(
                "session:test-owner", product.Id, null, Quantity: 3, IsWholesaleAuthorized: true),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*5*", "the error should reference the actual minimum quantity required");
    }

    [Fact]
    public async Task AddToCartCommand_WhenRetailCustomer_IgnoresWholesaleMinimumEntirely()
    {
        // A retail (non-wholesale) customer must be able to buy any quantity —
        // the minimum-quantity rule is wholesale-only and must not affect retail customers.
        await using var db = CreateInMemoryContext();

        var category = new ProductCategory { Name = "Cat", NameEn = "Cat", Slug = "cat2", Icon = "🧃", IsActive = true };
        db.ProductCategories.Add(category);

        var product = new Product
        {
            CategoryId = category.Id,
            Category = category,
            Sku = "CARTON-SKU-2",
            Slug = "carton-product-2",
            Name = "Carton Test 2",
            NameEn = "Carton Test 2",
            RetailPrice = 15000m,
            WholesalePrice = 12000m,
            MinimumWholesaleQuantity = 5,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var handler = new AYIGroupMarket.Application.Features.Cart.AddToCart.AddToCartCommandHandler(db);

        // Act — retail customer adds just 1 unit, below the wholesale minimum
        var cartId = await handler.Handle(
            new AYIGroupMarket.Application.Features.Cart.AddToCart.AddToCartCommand(
                "session:retail-owner", product.Id, null, Quantity: 1, IsWholesaleAuthorized: false),
            CancellationToken.None);

        // Assert — should succeed at retail price, no exception
        cartId.Should().NotBeEmpty();

        var cartItem = await db.CartItems.FirstAsync(i => i.ProductId == product.Id);
        cartItem.UnitPrice.Should().Be(15000m, "a retail customer must always pay retail price, never wholesale");
    }
}