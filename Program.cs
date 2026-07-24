using Stripe;
using Microsoft.EntityFrameworkCore;
using GymPayments.Endpoints;
using GymPayments.Data;
using GymPayments.Services;

// Builder
var builder = WebApplication.CreateBuilder(args);
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Services
builder.Services.AddDbContext<GymPaymentsContext>(options => options.UseSqlite("Data Source=gympayments.db"));
builder.Services.AddScoped<WebhookHandlerService>();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// App route mapping
app.UseHttpsRedirection();
app.MapCheckoutEndpoints();
app.MapWebhookEndpoints();
app.Run();