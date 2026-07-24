using Microsoft.EntityFrameworkCore;
using Stripe;
using GymPayments.Data;
using GymPayments.Models;

namespace GymPayments.Services;

public class WebhookHandlerService
{
    private readonly GymPaymentsContext _db;

    public WebhookHandlerService(GymPaymentsContext db)
    {
        _db = db;
    }

    public async Task HandleAsync(Event stripeEvent)
    {
        var alreadyProcessed = await _db.ProcessedWebhookEvents
            .AnyAsync(e => e.StripeEventId == stripeEvent.Id);

        if (alreadyProcessed) return;

        if (stripeEvent.Type == "checkout.session.completed")
        {
            await HandleCheckoutCompleted(stripeEvent);
        }
        else if (stripeEvent.Type == "invoice.payment_failed")
        {
            await HandlePaymentFailed(stripeEvent);
        }

        _db.ProcessedWebhookEvents.Add(new ProcessedWebhookEvent { StripeEventId = stripeEvent.Id });
        await _db.SaveChangesAsync();
    }

    private async Task HandleCheckoutCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        var email = session!.CustomerEmail ?? session.CustomerDetails?.Email ?? "unknown";
        var stripeCustomerId = session.CustomerId ?? "";

        var member = await _db.Members.FirstOrDefaultAsync(m => m.Email == email);
        if (member is null)
        {
            member = new Member
            {
                Id = Guid.NewGuid(),
                Name = email,
                Email = email,
                StripeCustomerId = stripeCustomerId
            };
            _db.Members.Add(member);
        }

        if (session.Mode == "subscription" && session.SubscriptionId is not null)
        {
            var subscriptionService = new Stripe.SubscriptionService();
            var subscription = await subscriptionService.GetAsync(session.SubscriptionId);

            _db.Memberships.Add(new Membership
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                StripeSubscriptionId = session.SubscriptionId,
                Status = subscription.Status,
                CurrentPeriodEnd = subscription.Items.Data.First().CurrentPeriodEnd
            });
        }

        _db.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            StripePaymentIntentId = session.PaymentIntentId,
            Amount = session.AmountTotal ?? 0,
            Currency = session.Currency ?? "eur",
            Status = "succeeded",
            Type = session.Mode == "subscription" ? "subscription" : "drop_in"
        });
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        var stripeCustomerId = invoice!.CustomerId;
        var member = await _db.Members.FirstOrDefaultAsync(m => m.StripeCustomerId == stripeCustomerId);

        if (member is null) return;

        var membership = await _db.Memberships.FirstOrDefaultAsync(m => m.MemberId == member.Id);
        if (membership is not null)
        {
            membership.Status = "past_due";
        }

        _db.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            StripeInvoiceId = invoice.Id,
            Amount = invoice.AmountDue,
            Currency = invoice.Currency ?? "eur",
            Status = "failed",
            Type = "subscription"
        });
    }
}