using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Products.ManageVariant;

public record UpsertVariantCommand(
    Guid? VariantId, Guid ProductId, string Sku, string Name, string NameEn,
    decimal RetailPrice, decimal? WholesalePrice, int? MinimumWholesaleQuantity) : IRequest<Guid>;

public class UpsertVariantCommandHandler(IApplicationDbContext db) : IRequestHandler<UpsertVariantCommand, Guid>
{
    public async Task<Guid> Handle(UpsertVariantCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.ProductVariant variant;

        if (request.VariantId.HasValue)
        {
            variant = await db.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.VariantId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Variant not found");
            variant.Sku = request.Sku;
            variant.Name = request.Name;
            variant.NameEn = request.NameEn;
        }
        else
        {
            variant = new Domain.Entities.ProductVariant
            {
                ProductId = request.ProductId,
                Sku = request.Sku,
                Name = request.Name,
                NameEn = request.NameEn,
                DisplayOrder = await db.ProductVariants.CountAsync(v => v.ProductId == request.ProductId, cancellationToken)
            };
            db.ProductVariants.Add(variant);
            await db.SaveChangesAsync(cancellationToken); // ensure variant.Id exists before referencing it in prices below
        }

        // Upsert Retail price row
        var retailPrice = await db.ProductPrices
            .FirstOrDefaultAsync(p => p.ProductVariantId == variant.Id && p.PriceType == PriceType.Retail, cancellationToken);
        if (retailPrice is null)
        {
            db.ProductPrices.Add(new ProductPrice { ProductVariantId = variant.Id, PriceType = PriceType.Retail, Amount = request.RetailPrice });
        }
        else
        {
            retailPrice.Amount = request.RetailPrice;
        }

        // Upsert Wholesale price row, if provided
        if (request.WholesalePrice.HasValue)
        {
            var wholesalePrice = await db.ProductPrices
                .FirstOrDefaultAsync(p => p.ProductVariantId == variant.Id && p.PriceType == PriceType.Wholesale, cancellationToken);
            if (wholesalePrice is null)
            {
                db.ProductPrices.Add(new ProductPrice
                {
                    ProductVariantId = variant.Id, PriceType = PriceType.Wholesale,
                    Amount = request.WholesalePrice.Value, MinimumQuantity = request.MinimumWholesaleQuantity
                });
            }
            else
            {
                wholesalePrice.Amount = request.WholesalePrice.Value;
                wholesalePrice.MinimumQuantity = request.MinimumWholesaleQuantity;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return variant.Id;
    }
}

public record DeleteVariantCommand(Guid VariantId) : IRequest;

public class DeleteVariantCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteVariantCommand>
{
    public async Task Handle(DeleteVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await db.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.VariantId, cancellationToken)
            ?? throw new KeyNotFoundException("Variant not found");

        db.ProductVariants.Remove(variant); // cascades to ProductPrices via the configured delete behavior
        await db.SaveChangesAsync(cancellationToken);
    }
}