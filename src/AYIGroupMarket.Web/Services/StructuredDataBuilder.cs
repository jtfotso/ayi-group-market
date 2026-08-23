using System.Text.Json;

namespace AYIGroupMarket.Web.Services;

public static class StructuredDataBuilder
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    public static string BuildOrganization(string baseUrl)
    {
        var data = new
        {
            context = "https://schema.org",
            type = "Organization",
            name = "AYI GROUP MARKET",
            url = baseUrl,
            logo = $"{baseUrl}/favicon.png",
            address = new
            {
                type = "PostalAddress",
                streetAddress = "Zone MAGZI Douala-Bassa, BP 7789",
                addressLocality = "Douala",
                addressCountry = "CM"
            },
            contactPoint = new
            {
                type = "ContactPoint",
                telephone = "+237695162019",
                contactType = "customer service"
            }
        };
        return SerializeWithAtKeys(data);
    }

    public static string BuildLocalBusiness(string baseUrl)
    {
        var data = new
        {
            context = "https://schema.org",
            type = "LocalBusiness",
            name = "AYI GROUP MARKET",
            url = baseUrl,
            telephone = "+237695162019",
            address = new
            {
                type = "PostalAddress",
                streetAddress = "Zone MAGZI Douala-Bassa, BP 7789",
                addressLocality = "Douala",
                addressCountry = "CM"
            }
        };
        return SerializeWithAtKeys(data);
    }

    public static string BuildBreadcrumbList(List<(string Name, string Url)> crumbs)
    {
        var itemListElement = crumbs.Select((c, i) => new
        {
            type = "ListItem",
            position = i + 1,
            name = c.Name,
            item = c.Url
        }).ToArray();

        var data = new
        {
            context = "https://schema.org",
            type = "BreadcrumbList",
            itemListElement
        };
        return SerializeWithAtKeys(data);
    }

    public static string BuildProduct(
        string name, string description, string imageUrl, string productUrl,
        decimal price, bool inStock, double? averageRating, int? reviewCount)
    {
        var offers = new Dictionary<string, object?>
        {
            ["@type"] = "Offer",
            ["url"] = productUrl,
            ["priceCurrency"] = "XAF",
            ["price"] = price,
            ["availability"] = inStock ? "https://schema.org/InStock" : "https://schema.org/OutOfStock"
        };

        var data = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Product",
            ["name"] = name,
            ["description"] = description,
            ["image"] = imageUrl,
            ["offers"] = offers
        };

        if (averageRating.HasValue && reviewCount is > 0)
        {
            data["aggregateRating"] = new Dictionary<string, object?>
            {
                ["@type"] = "AggregateRating",
                ["ratingValue"] = averageRating.Value,
                ["reviewCount"] = reviewCount
            };
        }

        return JsonSerializer.Serialize(data, Options);
    }

    // Helper: System.Text.Json can't easily emit "@type"/"@context" keys from anonymous object property
    // names (C# identifiers can't start with @ meaningfully here), so we serialize then string-replace
    // the placeholder property names ("type"/"context") with their real JSON-LD "@"-prefixed equivalents.
    private static string SerializeWithAtKeys(object data)
    {
        var json = JsonSerializer.Serialize(data, Options);
        return json
            .Replace("\"context\":", "\"@context\":")
            .Replace("\"type\":", "\"@type\":");
    }
}