using Stripe;
using GymPayments.Services;

namespace GymPayments.Endpoints;

public static class WebhookEndpoints
{
    public static void MapWebhookEndpoints(this WebApplication app)
    {
        app.MapPost("/webhook", async (HttpRequest request, WebhookHandlerService handler, IConfiguration config) =>
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();
            var webhookSecret = config["Stripe:WebhookSecret"];

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    request.Headers["Stripe-Signature"],
                    webhookSecret
                );
            }
            catch (StripeException e)
            {
                return Results.BadRequest($"Webhook signature verification failed: {e.Message}");
            }

            await handler.HandleAsync(stripeEvent);

            return Results.Ok();
        });
    }
}