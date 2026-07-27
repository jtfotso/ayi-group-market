using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace AYIGroupMarket.Infrastructure.Payments;

public class PaymentGatewayResolver(IServiceProvider serviceProvider) : IPaymentGatewayResolver
{
    public IPaymentGateway Resolve(PaymentMethod method) => method switch
    {
        PaymentMethod.MobileMoney => serviceProvider.GetRequiredService<MobileMoneyPaymentGateway>(),
        PaymentMethod.OrangeMoney => serviceProvider.GetRequiredService<OrangeMoneyPaymentGateway>(),
        PaymentMethod.CreditCard => serviceProvider.GetRequiredService<CardPaymentGateway>(),
        PaymentMethod.PayPal => serviceProvider.GetRequiredService<PayPalPaymentGateway>(),
        PaymentMethod.WhatsAppManual => serviceProvider.GetRequiredService<WhatsAppManualPaymentGateway>(),
        _ => throw new NotSupportedException($"No payment gateway registered for {method}")
    };
}