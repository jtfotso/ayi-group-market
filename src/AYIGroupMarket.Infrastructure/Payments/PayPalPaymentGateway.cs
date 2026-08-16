using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using Microsoft.Extensions.Options;

namespace AYIGroupMarket.Infrastructure.Payments;

public class PayPalPaymentGateway(HttpClient httpClient, IOptions<PayPalOptions> options) : IPaymentGateway
{
    private readonly PayPalOptions _options = options.Value;

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<PaymentResultDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            var orderPayload = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = request.OrderNumber,
                        amount = new
                        {
                            currency_code = "USD", // PayPal doesn't support XAF directly — see note below
                            value = ConvertXafToUsdPlaceholder(request.Amount)
                        }
                    }
                },
                application_context = new
                {
                    return_url = $"https://ayi-group-market-dev.azurewebsites.net/commande/paypal-return?orderNumber={request.OrderNumber}",
                    cancel_url = $"https://ayi-group-market-dev.azurewebsites.net/commande/paypal-cancel?orderNumber={request.OrderNumber}"
                }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/v2/checkout/orders");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(orderPayload), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[PayPal] Order creation failed. Status: {response.StatusCode}, Body: {responseJson}");
                    return new PaymentResultDto(false, null, null, $"PayPal order creation failed: {responseJson}");
                }

            using var doc = JsonDocument.Parse(responseJson);
            var paypalOrderId = doc.RootElement.GetProperty("id").GetString()!;

            var approveLink = doc.RootElement.GetProperty("links")
                .EnumerateArray()
                .First(l => l.GetProperty("rel").GetString() == "approve")
                .GetProperty("href").GetString();

            return new PaymentResultDto(true, paypalOrderId, approveLink, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PayPal] InitiatePaymentAsync failed: {ex}");
            return new PaymentResultDto(false, null, null, ex.Message);
        }
    }

    public async Task<PaymentVerificationResultDto> VerifyPaymentAsync(string transactionReference, CancellationToken cancellationToken)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            // transactionReference here is the PayPal Order ID captured at InitiatePaymentAsync
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/v2/checkout/orders/{transactionReference}/capture");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new PaymentVerificationResultDto(false, $"CAPTURE_FAILED: {responseJson}", null);

            using var doc = JsonDocument.Parse(responseJson);
            var status = doc.RootElement.GetProperty("status").GetString();

            if (status == "COMPLETED")
            {
                var capturedAmount = doc.RootElement
                    .GetProperty("purchase_units")[0]
                    .GetProperty("payments")
                    .GetProperty("captures")[0]
                    .GetProperty("amount")
                    .GetProperty("value").GetString();

                return new PaymentVerificationResultDto(true, "COMPLETED", decimal.Parse(capturedAmount!));
            }

            return new PaymentVerificationResultDto(false, status ?? "UNKNOWN", null);
        }
        catch (Exception ex)
        {
            return new PaymentVerificationResultDto(false, $"ERROR: {ex.Message}", null);
        }
    }

    // PLACEHOLDER: PayPal doesn't settle in XAF (Central African CFA franc) — it's not one of PayPal's
    // supported transaction currencies. This needs a real conversion strategy before going live:
    // either convert XAF→USD at checkout using a live exchange rate API, or restrict PayPal as a payment
    // option only for wholesale/international customers who'd be paying in USD/EUR anyway.
    // For now this just divides by a hardcoded rough rate so sandbox testing has a plausible amount.
    private static string ConvertXafToUsdPlaceholder(decimal xafAmount)
    {
        const decimal roughXafToUsdRate = 600m; // approximate, NOT for production use
        return (xafAmount / roughXafToUsdRate).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
    }
}