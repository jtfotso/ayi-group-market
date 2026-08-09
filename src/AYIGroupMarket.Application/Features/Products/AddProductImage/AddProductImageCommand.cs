using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Products.AddProductImage;

public record AddProductImageCommand(Guid ProductId, string Url, bool IsPrimary) : IRequest<Guid>;

public class AddProductImageCommandHandler(IApplicationDbContext db) : IRequestHandler<AddProductImageCommand, Guid>
{
    public async Task<Guid> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        if (request.IsPrimary)
        {
            // only one primary image per product — clear any existing primary flag first
            var existingPrimary = await db.ProductImages
                .Where(i => i.ProductId == request.ProductId && i.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var img in existingPrimary)
                img.IsPrimary = false;
        }

        var image = new ProductImage
        {
            ProductId = request.ProductId,
            Url = request.Url,
            IsPrimary = request.IsPrimary,
            DisplayOrder = await db.ProductImages.CountAsync(i => i.ProductId == request.ProductId, cancellationToken)
        };

        db.ProductImages.Add(image);
        await db.SaveChangesAsync(cancellationToken);
        return image.Id;
    }
}