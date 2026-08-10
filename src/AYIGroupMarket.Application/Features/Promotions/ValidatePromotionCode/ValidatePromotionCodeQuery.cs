using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using AYIGroupMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Promotions.ValidatePromotionCode;

// IsWholesaleAuthorized derived server-side, never client-supplied — same discipline as pricing checks
public record ValidatePromotionCodeQuery(
    string Code, decimal CartSubtotal, int CartTotalQuantity, bool IsWholesaleAuthorized) : IRequest<ValidatePromotionResultDto>;

public class ValidatePromotionCodeQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ValidatePromotionCodeQuery, ValidatePromotionResultDto>
{
    public async Task<ValidatePromotionResultDto> Handle(ValidatePromotionCodeQuery request, CancellationToken cancellationToken)
    {
        var promo = await db.Promotions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == request.Code.ToUpper() && p.IsActive, cancellationToken);

        if (promo is null)
            return new ValidatePromotionResultDto(false, "Invalid promo code.", 0);

        var now = DateTime.UtcNow;
        if (now < promo.StartDate || now > promo.EndDate)
            return new ValidatePromotionResultDto(false, "This promo code has expired or is not yet active.", 0);

        if (promo.MaxUses.HasValue && promo.UsesCount >= promo.MaxUses.Value)
            return new ValidatePromotionResultDto(false, "This promo code has reached its usage limit.", 0);

        if (promo.MinimumOrderAmount.HasValue && request.CartSubtotal < promo.MinimumOrderAmount.Value)
            return new ValidatePromotionResultDto(false,
                $"Minimum order amount for this code is {promo.MinimumOrderAmount.Value:C}.", 0);

        if (promo.MinimumQuantity.HasValue && request.CartTotalQuantity < promo.MinimumQuantity.Value)
            return new ValidatePromotionResultDto(false,
                $"Minimum quantity for this code is {promo.MinimumQuantity.Value}.", 0);

        if (promo.CustomerType == PromotionCustomerType.WholesaleOnly && !request.IsWholesaleAuthorized)
            return new ValidatePromotionResultDto(false, "This code is reserved for wholesale customers.", 0);

        if (promo.CustomerType == PromotionCustomerType.RetailOnly && request.IsWholesaleAuthorized)
            return new ValidatePromotionResultDto(false, "This code is reserved for retail customers.", 0);

        var discount = promo.Type switch
        {
            PromotionType.PercentageDiscount => request.CartSubtotal * (promo.DiscountPercentage ?? 0) / 100m,
            PromotionType.FixedAmount or PromotionType.CartDiscount => promo.DiscountAmount ?? 0,
            _ => 0m // ProductSpecific/CategoryDiscount need per-item application, not just a flat cart discount — see note below
        };

        // Never let the discount exceed the subtotal
        discount = Math.Min(discount, request.CartSubtotal);

        return new ValidatePromotionResultDto(true, null, discount);
    }
}