using System.Globalization;

namespace AYIGroupMarket.Web.Services;

public static class CurrencyFormatter
{
    // XAF (Central African CFA franc) has no decimal subdivision in practice —
    // amounts are always whole numbers, so we format accordingly.
    public static string Format(decimal amount)
    {
        return $"{amount.ToString("N0", CultureInfo.InvariantCulture)} FCFA";
    }
}