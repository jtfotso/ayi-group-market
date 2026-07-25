using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AYIGroupMarket.Application.Features.Categories.CreateCategory;

public record CreateCategoryCommand(
    string Name, string NameEn, string Slug, string Icon, int DisplayOrder, bool IsActive) : IRequest<Guid>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
    }
}

public class CreateCategoryCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new ProductCategory
        {
            Name = request.Name,
            NameEn = request.NameEn,
            Slug = request.Slug,
            Icon = request.Icon,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}