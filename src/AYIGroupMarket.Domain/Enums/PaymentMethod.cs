namespace AYIGroupMarket.Domain.Enums;

public enum PaymentMethod
{
    PayUnit = 0,       // covers Mobile Money, Orange Money, and card — all via PayUnit's hosted page
    WhatsAppManual = 1,
    PayPal = 2
}