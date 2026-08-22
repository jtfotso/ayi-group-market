using System.Globalization;
using System.Web;

namespace AYIGroupMarket.Web.Services;

public static class WhatsAppLinkBuilder
{
    private const string PhoneNumber = "237695162019"; // no + or spaces, per wa.me format
    public static string BuildGenericLink(bool isFrench)
    {
        var message = isFrench
            ? "Bonjour AYI GROUP MARKET, je souhaite passer une commande."
            : "Hello AYI GROUP MARKET, I would like to place an order.";

        return $"https://wa.me/{PhoneNumber}?text={HttpUtility.UrlEncode(message)}";
    }

    public static string BuildProductLink(bool isFrench, string productName, int quantity = 1, string? variantName = null)
    {
        var fullProductName = variantName is not null ? $"{productName} — {variantName}" : productName;

        var message = isFrench
            ? $"Bonjour AYI GROUP MARKET,\n\nJe souhaite commander le produit suivant :\n\nProduit : {fullProductName}\nQuantité : {quantity}\n\nMerci de me confirmer la disponibilité."
            : $"Hello AYI GROUP MARKET,\n\nI would like to order the following product:\n\nProduct: {fullProductName}\nQuantity: {quantity}\n\nPlease confirm availability.";

        return $"https://wa.me/{PhoneNumber}?text={HttpUtility.UrlEncode(message)}";
    }

    public static string BuildCartLink(bool isFrench, List<(string Name, int Quantity)> items)
    {
        var itemLines = string.Join("\n", items.Select(i => $"- {i.Name} x{i.Quantity}"));

        var message = isFrench
            ? $"Bonjour AYI GROUP MARKET,\n\nJe souhaite commander les produits suivants :\n\n{itemLines}\n\nMerci de me confirmer la disponibilité et le prix total."
            : $"Hello AYI GROUP MARKET,\n\nI would like to order the following products:\n\n{itemLines}\n\nPlease confirm availability and total price.";

        return $"https://wa.me/{PhoneNumber}?text={HttpUtility.UrlEncode(message)}";
    }
}