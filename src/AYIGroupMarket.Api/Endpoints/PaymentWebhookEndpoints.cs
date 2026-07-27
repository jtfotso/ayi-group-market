using AYIGroupMarket.Application.Features.Payments.VerifyPayment;
using MediatR;

namespace AYIGroupMarket.Api.Endpoints;

public static class PaymentWebhookEndpoints
{
    public static void MapPaymentWebhookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/webhooks/payments").WithTags("Payment Webhooks");

        // One route per provider — each provider's webhook payload shape differs,
        // so a shared endpoint would need provider-specific parsing anyway.
        group.MapPost("/mobile-money", async (ISender sender, HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var rawBody = await reader.ReadToEndAsync();

            // PLACEHOLDER: parse rawBody per the real Mobile Money provider's webhook schema
            // and extract the transaction reference — this is provider-specific and can't be
            // finalized until you've picked an aggregator (Campay, MTN MoMo API, etc.)
            return Results.Ok();
        });

        group.MapPost("/orange-money", async (HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var rawBody = await reader.ReadToEndAsync();
            // PLACEHOLDER: same as above, Orange Money's own webhook schema
            return Results.Ok();
        });

        group.MapPost("/card", async (HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var rawBody = await reader.ReadToEndAsync();
            // PLACEHOLDER: card processor's webhook schema (e.g. Stripe signature verification)
            return Results.Ok();
        });

        group.MapPost("/paypal", async (HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var rawBody = await reader.ReadToEndAsync();
            // PLACEHOLDER: PayPal webhook signature verification + event parsing
            return Results.Ok();
        });
    }
}