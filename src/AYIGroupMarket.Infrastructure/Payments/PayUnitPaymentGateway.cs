using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Application.DTOs;
using Microsoft.Extensions.Options;

namespace AYIGroupMarket.Infrastructure.Payments;

// Covers both MTN Mobile Money and Orange Money — PayUnit is a single unified gateway
// that routes to whichever provider the customer selects on its hosted payment page.
public class PayUnitPaymentGateway(HttpClient httpClient, IOptions<PayUnitOptions> options, IOptions<AppOptions> appOptions) : IPaymentGateway
{
    private readonly PayUnitOptions _options = options.Value;
    private readonly string _baseUrl = appOptions.Value.BaseUrl;

    private void ApplyAuthHeaders(HttpRequestMessage request)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ApiUser}:{_options.ApiPassword}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Add("mode", _options.Mode);
    }

    public async Task<PaymentResultDto> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Append a short unique suffix so retries (e.g. after a transient failure) never collide
            // with a previous attempt's transaction_id on PayUnit's side
            var payUnitTransactionId = $"{request.OrderNumber}-{DateTime.UtcNow:HHmmss}{Guid.NewGuid().ToString()[..4]}";

            var payload = new
            {
                total_amount = (int)request.Amount,
                currency = "XAF",
                transaction_id = payUnitTransactionId,
                return_url = $"{_baseUrl}/commande/payunit-return?orderNumber={request.OrderNumber}",
                notify_url = $"{_baseUrl}/api/webhooks/payments/payunit",
                payment_country = "CM"
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/api/gateway/initialize");
            ApplyAuthHeaders(httpRequest);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new PaymentResultDto(false, null, null, $"PayUnit initialization failed: {responseJson}");

            using var doc = JsonDocument.Parse(responseJson);
            var data = doc.RootElement.GetProperty("data");
            var transactionUrl = data.GetProperty("transaction_url").GetString();

            // Store PayUnit's own transaction ID (not the order number) as our reference for status checks
            return new PaymentResultDto(true, payUnitTransactionId, transactionUrl, null);
        }
        catch (Exception ex)
        {
            return new PaymentResultDto(false, null, null, ex.Message);
        }
    }

    public async Task<PaymentVerificationResultDto> VerifyPaymentAsync(string transactionReference, CancellationToken cancellationToken)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_options.BaseUrl}/api/gateway/paymentstatus/{transactionReference}");
            ApplyAuthHeaders(httpRequest);

            var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new PaymentVerificationResultDto(false, "STATUS_CHECK_FAILED", null);

            using var doc = JsonDocument.Parse(responseJson);
            var data = doc.RootElement.GetProperty("data");
            var status = data.GetProperty("transaction_status").GetString();
            var amount = data.GetProperty("transaction_amount").GetDecimal();

            return status switch
            {
                "SUCCESS" => new PaymentVerificationResultDto(true, "SUCCESS", amount),
                _ => new PaymentVerificationResultDto(false, status ?? "UNKNOWN", null)
            };
        }
        catch (Exception ex)
        {
            return new PaymentVerificationResultDto(false, $"ERROR: {ex.Message}", null);
        }
    }
}