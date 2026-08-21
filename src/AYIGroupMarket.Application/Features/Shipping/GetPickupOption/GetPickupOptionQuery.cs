using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Shipping.GetPickupOption;

public record GetPickupOptionQuery : IRequest<ShippingRateDto?>;

public class GetPickupOptionQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPickupOptionQuery, ShippingRateDto?>
{
    public async Task<ShippingRateDto?> Handle(GetPickupOptionQuery request, CancellationToken cancellationToken)
    {
        return await db.ShippingRates.AsNoTracking()
            .Where(r => r.IsPickup && r.IsActive)
            .Select(r => new ShippingRateDto(
                r.Id, r.DeliveryMethod, r.DeliveryMethodEn, r.IsPickup, r.DeliveryDays,
                r.BaseFee, r.FeePerKg, r.FreeShippingThreshold))
            .FirstOrDefaultAsync(cancellationToken);
    }
}