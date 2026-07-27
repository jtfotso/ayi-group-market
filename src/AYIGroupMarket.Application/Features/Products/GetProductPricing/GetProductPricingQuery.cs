using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Products.GetProductPricing;

// IsWholesaleAuthorized is passed in by the CALLER (Web/Api layer), which must derive it
// from the actual authenticated ClaimsPrincipal's role — never from a client-supplied flag.
public record GetProductPricingQuery(Guid ProductId, bool IsWholesaleAuthorized) : IRequest<ProductPricingDto>;

public class GetProductPricingQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductPricingQuery, ProductPricingDto>
{
    public async Task<ProductPricingDto> Handle(GetProductPricingQuery request, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Product not found");

        return new ProductPricingDto(
            product.Id,
            product.RetailPrice,
            request.IsWholesaleAuthorized ? product.WholesalePrice : null,
            request.IsWholesaleAuthorized ? product.MinimumWholesaleQuantity : null);
    }
}