using AYIGroupMarket.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Products.DeleteProductImage;

public record DeleteProductImageCommand(Guid ImageId) : IRequest<string>; // returns the Url so the caller can delete the physical file

public class DeleteProductImageCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteProductImageCommand, string>
{
    public async Task<string> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await db.ProductImages.FirstOrDefaultAsync(i => i.Id == request.ImageId, cancellationToken)
            ?? throw new KeyNotFoundException("Image not found");

        var url = image.Url;
        db.ProductImages.Remove(image);
        await db.SaveChangesAsync(cancellationToken);

        return url;
    }
}