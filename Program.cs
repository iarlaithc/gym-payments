using Stripe;
using Microsoft.EntityFrameworkCore;
using GymPayments.Models;
using GymPayments.Data;
using GymPayments;

// Builder
var builder = WebApplication.CreateBuilder(args);
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Services
builder.Services.AddDbContext<GymPaymentsContext>(options => options.UseSqlite("Data Source=gympayments.db"));
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

/// Endpoints testing
app.MapGet("/stripe-test", async () =>
{
    var service = new Stripe.BalanceService();
    var balance = await service.GetAsync();
    return Results.Ok(balance);
});

app.MapPost("/checkout/{type}", async (string type, string email, IConfiguration config) =>
{
    if (type != "membership" && type != "drop_in")
        return Results.BadRequest("type must be 'membership' or 'drop_in'");

    var priceId = type == "membership"
        ? config["Stripe:MembershipPriceId"]
        : config["Stripe:DropInPriceId"];

    var options = new Stripe.Checkout.SessionCreateOptions
    {
        Mode = type == "membership" ? "subscription" : "payment",
        LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
        {
            new() { Price = priceId, Quantity = 1 }
        },
        CustomerEmail = email,
        SuccessUrl = "http://localhost:5228/success?session_id={CHECKOUT_SESSION_ID}",
        CancelUrl = "http://localhost:5228/cancel"
    };

    var service = new Stripe.Checkout.SessionService();
    var session = await service.CreateAsync(options);

    return Results.Ok(new { checkoutUrl = session.Url });
});

// Webhook 
app.MapPost("/webhook", async (HttpRequest request, GymPaymentsContext db, IConfiguration config) =>
{
    var json = await new StreamReader(request.Body).ReadToEndAsync();
    var webhookSecret = config["Stripe:WebhookSecret"];

    Event stripeEvent;
    try
    {
        // verifies the signature
        stripeEvent = EventUtility.ConstructEvent(
            json,
            request.Headers["Stripe-Signature"],
            webhookSecret
        );
    }
    catch (StripeException e)
    {
        return Results.BadRequest($"Webhook siognature verification failed: {e.Message}");
    }

    // check if event already processed
    var alreadyProcessed = await db.ProcessedWebhookEvents.AnyAsync(e => e.StripeEventId == stripeEvent.Id);

    if (alreadyProcessed) return Results.Ok();

    // positive event
    if (stripeEvent.Type == "checkout.session.completed")
    {
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        var email = session!.CustomerEmail ?? session.CustomerDetails?.Email ?? "unknown";
        var stripeCustomerId = session.CustomerId ?? "";

        // find or create member
        var member = await db.Members.FirstOrDefaultAsync(m => m.Email == email);

        if (member is null)
        {
            member = new Member
            {
                Id = Guid.NewGuid(),
                Name = email, // placeholder
                Email = email,
                StripeCustomerId = stripeCustomerId
            };
            db.Members.Add(member);
        }

        if (session.Mode == "subscription" && session.SubscriptionId is not null)
        {
            var subscriptionService = new Stripe.SubscriptionService();
            var subscription = await subscriptionService.GetAsync(session.SubscriptionId);

            db.Memberships.Add(new Membership
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                StripeSubscriptionId = session.SubscriptionId,
                Status = subscription.Status,
                CurrentPeriodEnd = subscription.Items.Data.First().CurrentPeriodEnd
            });
        }        

        // record payment in db
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            StripePaymentIntentId = session.PaymentIntentId,
            Amount = session.AmountTotal ?? 0,
            Currency = session.Currency ?? "eur",
            Status = "succeeded",
            Type = session.Mode == "subscription" ? "subscription" : "drop_in"
        };

        db.Payments.Add(payment);
    }
    else if (stripeEvent.Type == "invoice.payment_failed")
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        var stripeCustomerId = invoice!.CustomerId;
        var member = await db.Members.FirstOrDefaultAsync(m => m.StripeCustomerId == stripeCustomerId);
        if (member is not null)
        {
            var membership = await db.Memberships.FirstOrDefaultAsync(m => m.MemberId == member.Id);
            
            if (membership is not null)
            {
                membership.Status = "past_due";
            }

            db.Payments.Add(new Payment
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                StripeInvoiceId = invoice.Id,
                Amount = invoice.AmountDue,
                Currency = invoice.Currency ?? "eur",
                Type = "subscription"
            });
        }
    }

    db.ProcessedWebhookEvents.Add(new ProcessedWebhookEvent {StripeEventId = stripeEvent.Id});

    await db.SaveChangesAsync();

    return Results.Ok();
});

app.Run();