using AYIGroupMarket.Domain.Enums;

namespace AYIGroupMarket.Application.Abstractions;

public interface IPaymentGatewayResolver
{
    IPaymentGateway Resolve(PaymentMethod method);
}