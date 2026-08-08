using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
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
    string? Notes) : IRequest<OrderDto>;

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

public class CreateOrderCommandHandler(IApplicationDbContext db, IOrderNumberGenerator orderNumberGenerator, INotificationService notificationService)
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
            DiscountAmount = 0,
            TaxAmount = 0,
            Total = subtotal + shippingFee,
            Notes = request.Notes
        };

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