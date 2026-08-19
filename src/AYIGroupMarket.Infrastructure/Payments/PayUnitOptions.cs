namespace AYIGroupMarket.Infrastructure.Payments;

public class PayUnitOptions
{
    public string ApiUser { get; set; } = string.Empty;
    public string ApiPassword { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Mode { get; set; } = "test"; // "test" or "live"
    public string BaseUrl { get; set; } = "https://gateway.payunit.net";
}