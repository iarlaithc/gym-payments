using Stripe;
using Microsoft.EntityFrameworkCore;
using GymPayments.Models;
using GymPayments.Data;

// Builder
var builder = WebApplication.CreateBuilder(args);
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Services
builder.Services.AddDbContext<GymPaymentsContext>(options => options.UseSqlite("Data Source=gympayments.db"));
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

///testing
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

app.Run();