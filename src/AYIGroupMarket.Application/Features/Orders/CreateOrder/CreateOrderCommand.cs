using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using AYIGroupMarket.Application.Features.Promotions.ValidatePromotionCode;
using AYIGroupMarket.Domain.Entities;
using AYIGroupMarket.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AYIGroupMarket.Application.Features.Orders.CreateOrder;

public record CreateOrderCommand(
    string OwnerKey,
    CreateAddressRequest Address,
    Guid ShippingRateId,
    PaymentMethod PaymentMethod,
    string? Notes,
    string? PromoCode,
    bool IsWholesaleAuthorized) : IRequest<OrderDto>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OwnerKey).NotEmpty();
        RuleFor(x => x.ShippingRateId).NotEmpty();
        RuleFor(x => x.Address.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Address.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Address.AddressLine).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Address.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address.ShippingZoneId).NotEmpty();
    }
}

public class CreateOrderCommandHandler(IApplicationDbContext db, IOrderNumberGenerator orderNumberGenerator, INotificationService notificationService,
    ISender sender)
    : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var cart = await db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.OwnerKey == request.OwnerKey, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            throw new InvalidOperationException("Cannot create an order from an empty cart.");

        var shippingRate = await db.ShippingRates.FirstOrDefaultAsync(r => r.Id == request.ShippingRateId, cancellationToken)
            ?? throw new KeyNotFoundException("Shipping rate not found");

        var address = new Address
        {
            FullName = request.Address.FullName,
            Phone = request.Address.Phone,
            Email = request.Address.Email,
            AddressLine = request.Address.AddressLine,
            City = request.Address.City,
            ShippingZoneId = request.Address.ShippingZoneId
        };
        db.Addresses.Add(address);

        var subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

        var shippingFee = shippingRate.FreeShippingThreshold.HasValue && subtotal >= shippingRate.FreeShippingThreshold.Value
            ? 0
            : shippingRate.BaseFee;

        var orderNumber = await orderNumberGenerator.GenerateAsync(cancellationToken);

        decimal discountAmount = 0;
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promoResult = await sender.Send(
                new AYIGroupMarket.Application.Features.Promotions.ValidatePromotionCode.ValidatePromotionCodeQuery(
                    request.PromoCode, subtotal, cart.Items.Sum(i => i.Quantity), request.IsWholesaleAuthorized),
                cancellationToken);

            if (promoResult.IsValid)
                discountAmount = promoResult.DiscountAmount;
            // if invalid at this point (e.g. expired between UI check and submit), silently apply no discount
            // rather than failing the whole order — arguably should surface an error instead; worth deciding later
        }

        var order = new Order
        {
            OrderNumber = orderNumber,
            OwnerKey = request.OwnerKey,
            Address = address,
            ShippingRateId = shippingRate.Id,
            PaymentMethod = request.PaymentMethod,
            Status = OrderStatus.PaymentPending,
            Subtotal = subtotal,
            ShippingFee = shippingFee,
            DiscountAmount = discountAmount,
            TaxAmount = 0,
            Total = subtotal + shippingFee - discountAmount,
            PromoCode = request.PromoCode,
            Notes = request.Notes
        };

        if (!string.IsNullOrWhiteSpace(request.PromoCode) && discountAmount > 0)
        {
            var promo = await db.Promotions.FirstOrDefaultAsync(p => p.Code == request.PromoCode.ToUpper(), cancellationToken);
            if (promo is not null)
                promo.UsesCount++;
        }

        foreach (var cartItem in cart.Items)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                ProductVariantId = cartItem.ProductVariantId,
                ProductNameSnapshot = cartItem.Product.Name,
                ProductNameEnSnapshot = cartItem.Product.NameEn,
                VariantNameSnapshot = cartItem.ProductVariant?.Name,
                VariantNameEnSnapshot = cartItem.ProductVariant?.NameEn,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity,
                LineTotal = cartItem.UnitPrice * cartItem.Quantity
            };

            db.OrderItems.Add(orderItem);

            // Decrement stock and log the transaction — product-level stock only;
            // variant-level stock tracking isn't modeled yet (ProductVariant has no StockQuantity field currently)
            cartItem.Product.StockQuantity -= cartItem.Quantity;

            db.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = cartItem.ProductId,
                Type = InventoryTransactionType.Sale,
                QuantityChange = -cartItem.Quantity,
                ResultingStock = cartItem.Product.StockQuantity,
                Reason = $"Order {orderNumber}"
            });
        }

        db.Orders.Add(order);

        // Clear the cart now that its contents have been converted into an order
        foreach (var item in cart.Items.ToList())
            db.CartItems.Remove(item);

        await db.SaveChangesAsync(cancellationToken);

        if (request.OwnerKey.StartsWith("user:"))
        {
            var userId = request.OwnerKey["user:".Length..];
            await notificationService.NotifyAsync(
                userId,
                "Commande confirmée", "Order confirmed",
                $"Votre commande {order.OrderNumber} a été enregistrée.", $"Your order {order.OrderNumber} has been placed.",
                "/mon-compte/commandes", cancellationToken);
        }

        var itemDtos = cart.Items.Select(i => new OrderItemDto(
            i.Product.Name, i.Product.NameEn,
            i.ProductVariant?.Name, i.ProductVariant?.NameEn,
            i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity
        )).ToList();

        return new OrderDto(order.Id, order.OrderNumber, order.Status.ToString(),
            order.Subtotal, order.ShippingFee, order.Total, itemDtos);
    }
}