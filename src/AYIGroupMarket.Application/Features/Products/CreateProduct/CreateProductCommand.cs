using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AYIGroupMarket.Application.Features.Products.CreateProduct;

public record CreateProductCommand(
    string Sku, string Slug, Guid CategoryId, string Name, string NameEn,
    string ShortDescription, string ShortDescriptionEn,
    string Description, string DescriptionEn,
    decimal RetailPrice, decimal? WholesalePrice, int? MinimumWholesaleQuantity,
    int StockQuantity, bool IsActive, bool IsFeatured) : IRequest<Guid>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.RetailPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Sku = request.Sku,
            Slug = request.Slug,
            CategoryId = request.CategoryId,
            Name = request.Name,
            NameEn = request.NameEn,
            ShortDescription = request.ShortDescription,
            ShortDescriptionEn = request.ShortDescriptionEn,
            Description = request.Description,
            DescriptionEn = request.DescriptionEn,
            RetailPrice = request.RetailPrice,
            WholesalePrice = request.WholesalePrice,
            MinimumWholesaleQuantity = request.MinimumWholesaleQuantity,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}