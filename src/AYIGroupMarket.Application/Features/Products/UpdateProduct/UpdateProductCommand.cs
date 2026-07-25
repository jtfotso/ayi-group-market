using AYIGroupMarket.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Products.UpdateProduct;

public record UpdateProductCommand(
    Guid Id, string Sku, string Slug, Guid CategoryId, string Name, string NameEn,
    string ShortDescription, string ShortDescriptionEn,
    string Description, string DescriptionEn,
    decimal RetailPrice, decimal? WholesalePrice, int? MinimumWholesaleQuantity,
    int StockQuantity, bool IsActive, bool IsFeatured) : IRequest;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.RetailPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found");

        product.Sku = request.Sku;
        product.Slug = request.Slug;
        product.CategoryId = request.CategoryId;
        product.Name = request.Name;
        product.NameEn = request.NameEn;
        product.ShortDescription = request.ShortDescription;
        product.ShortDescriptionEn = request.ShortDescriptionEn;
        product.Description = request.Description;
        product.DescriptionEn = request.DescriptionEn;
        product.RetailPrice = request.RetailPrice;
        product.WholesalePrice = request.WholesalePrice;
        product.MinimumWholesaleQuantity = request.MinimumWholesaleQuantity;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}