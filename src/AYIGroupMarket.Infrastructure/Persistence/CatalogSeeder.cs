using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Infrastructure.Persistence;

public static class CatalogSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.ProductCategories.AnyAsync())
            return; // already seeded

        var agroalimentaire = new ProductCategory
        {
            Name = "Agroalimentaire",
            NameEn = "Food & Groceries",
            Slug = "agroalimentaire",
            Icon = "🌿",
            DisplayOrder = 1,
            IsActive = true
        };

        var parfumsMaison = new ProductCategory
        {
            Name = "Parfums & Maison",
            NameEn = "Fragrances & Home",
            Slug = "parfums-maison",
            Icon = "🏡",
            DisplayOrder = 2,
            IsActive = true
        };

        var entretienAuto = new ProductCategory
        {
            Name = "Entretien Automobile",
            NameEn = "Car Care",
            Slug = "entretien-automobile",
            Icon = "🚗",
            DisplayOrder = 3,
            IsActive = true
        };

        var electromenager = new ProductCategory
        {
            Name = "Électroménager",
            NameEn = "Home Appliances",
            Slug = "electromenager",
            Icon = "🔌",
            DisplayOrder = 4,
            IsActive = false // "Bientôt disponible"
        };

        var sanitaires = new ProductCategory
        {
            Name = "Sanitaires & Carrelage",
            NameEn = "Bathroom & Tiling",
            Slug = "sanitaires-carrelage",
            Icon = "🚿",
            DisplayOrder = 5,
            IsActive = false // "Bientôt disponible"
        };

        db.ProductCategories.AddRange(agroalimentaire, parfumsMaison, entretienAuto, electromenager, sanitaires);
        await db.SaveChangesAsync(); // persist categories first so Ids exist for FK references below

        // ---- Jus FRUILEG (variants: 33 cl / 50 cl / Carton 24 unités) ----
        var fruilegFlavors = new[]
        {
            ("Pur Jus Ananas", "Pure Pineapple Juice", "FRUILEG-ANANAS"),
            ("Ananas Gingembre Lemon", "Pineapple Ginger Lemon", "FRUILEG-GINGEMBRE"),
            ("Ananas Cassis", "Pineapple Blackcurrant", "FRUILEG-CASSIS"),
            ("Ananas Mangue", "Pineapple Mango", "FRUILEG-MANGUE"),
            ("Ananas Menthe", "Pineapple Mint", "FRUILEG-MENTHE"),
            ("Ananas Passion", "Pineapple Passion Fruit", "FRUILEG-PASSION"),
            ("Ananas Carotte", "Pineapple Carrot", "FRUILEG-CAROTTE"),
            ("Ananas Mandarine", "Pineapple Mandarin", "FRUILEG-MANDARINE"),
            ("Ananas Betterave", "Pineapple Beetroot", "FRUILEG-BETTERAVE"),
            ("Goyave", "Guava", "FRUILEG-GOYAVE"),
        };

        var products = new List<Product>();

        foreach (var (nameFr, nameEn, skuPrefix) in fruilegFlavors)
        {
            var product = new Product
            {
                Sku = skuPrefix,
                Slug = skuPrefix.ToLowerInvariant(),
                CategoryId = agroalimentaire.Id,
                Name = nameFr,
                NameEn = nameEn,
                ShortDescription = "Jus 100% naturel FRUILEG",
                ShortDescriptionEn = "100% natural FRUILEG juice",
                Description = $"{nameFr} — jus 100% naturel, sans sucres ajoutés, produit au Cameroun.",
                DescriptionEn = $"{nameEn} — 100% natural juice, no added sugar, made in Cameroon.",
                RetailPrice = 0, // to be set via Admin Dashboard per spec section 9
                IsActive = true
            };

            product.Variants.Add(new ProductVariant
            {
                Sku = $"{skuPrefix}-33CL",
                Name = "33 cl",
                NameEn = "33 cl",
                DisplayOrder = 1
            });
            product.Variants.Add(new ProductVariant
            {
                Sku = $"{skuPrefix}-50CL",
                Name = "50 cl",
                NameEn = "50 cl",
                DisplayOrder = 2
            });
            product.Variants.Add(new ProductVariant
            {
                Sku = $"{skuPrefix}-CARTON24",
                Name = "Carton 24 unités",
                NameEn = "Carton of 24",
                DisplayOrder = 3
            });

            products.Add(product);
        }

        // ---- Snacks ----
        products.Add(new Product
        {
            Sku = "SNACK-CHIPS-PLANTAIN",
            Slug = "chips-plantain",
            CategoryId = agroalimentaire.Id,
            Name = "Chips Plantain",
            NameEn = "Plantain Chips",
            ShortDescription = "Chips de plantain croustillantes",
            ShortDescriptionEn = "Crispy plantain chips",
            Description = "Chips de plantain croustillantes, préparées à partir de plantains sélectionnés.",
            DescriptionEn = "Crispy plantain chips made from selected plantains.",
            RetailPrice = 0,
            IsActive = true
        });

        products.Add(new Product
        {
            Sku = "SNACK-CHIPS-PATATE",
            Slug = "chips-patate-douce",
            CategoryId = agroalimentaire.Id,
            Name = "Chips Patate Douce",
            NameEn = "Sweet Potato Chips",
            ShortDescription = "Chips de patate douce",
            ShortDescriptionEn = "Sweet potato chips",
            Description = "Chips de patate douce, une alternative saine et savoureuse.",
            DescriptionEn = "Sweet potato chips, a healthy and tasty alternative.",
            RetailPrice = 0,
            IsActive = true
        });

        // ---- Superfoods ----
        products.Add(new Product
        {
            Sku = "SUPERFOOD-CHIA-BIO",
            Slug = "graines-de-chia-bio",
            CategoryId = agroalimentaire.Id,
            Name = "Graines de Chia BIO",
            NameEn = "Organic Chia Seeds",
            ShortDescription = "Graines de chia biologiques",
            ShortDescriptionEn = "Organic chia seeds",
            Description = "Graines de chia 100% biologiques, riches en oméga-3 et en fibres.",
            DescriptionEn = "100% organic chia seeds, rich in omega-3 and fiber.",
            RetailPrice = 0,
            IsActive = true
        });

        // ---- Parfums & Maison ----
        products.Add(new Product
        {
            Sku = "HOME-AER",
            Slug = "aer",
            CategoryId = parfumsMaison.Id,
            Name = "AER",
            NameEn = "AER",
            ShortDescription = "Désodorisant d'intérieur",
            ShortDescriptionEn = "Home air freshener",
            Description = "Désodorisant AER pour un intérieur toujours frais et parfumé.",
            DescriptionEn = "AER air freshener for a consistently fresh, fragrant home.",
            RetailPrice = 0,
            IsActive = true
        });

        products.Add(new Product
        {
            Sku = "HOME-ABRO-ORGANIC",
            Slug = "abro-organic",
            CategoryId = parfumsMaison.Id,
            Name = "ABRO Organic",
            NameEn = "ABRO Organic",
            ShortDescription = "Gamme de produits d'entretien naturels",
            ShortDescriptionEn = "Natural home care range",
            Description = "ABRO Organic — gamme de produits d'entretien à base d'ingrédients naturels.",
            DescriptionEn = "ABRO Organic — home care range made from natural ingredients.",
            RetailPrice = 0,
            IsActive = true
        });

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}