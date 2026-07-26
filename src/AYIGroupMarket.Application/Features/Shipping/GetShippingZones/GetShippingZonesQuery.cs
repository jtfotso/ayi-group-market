using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Shipping.GetShippingZones;

public record GetShippingZonesQuery : IRequest<List<ShippingZoneDto>>;

public class GetShippingZonesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetShippingZonesQuery, List<ShippingZoneDto>>
{
    public async Task<List<ShippingZoneDto>> Handle(GetShippingZonesQuery request, CancellationToken cancellationToken)
    {
        return await db.ShippingZones.AsNoTracking()
            .Where(z => z.IsActive)
            .Select(z => new ShippingZoneDto(
                z.Id, z.Name, z.NameEn,
                z.Rates.Where(r => r.IsActive).Select(r => new ShippingRateDto(
                    r.Id, r.DeliveryMethod, r.DeliveryMethodEn, r.BaseFee, r.FeePerKg, r.FreeShippingThreshold
                )).ToList()
            ))
            .ToListAsync(cancellationToken);
    }
}