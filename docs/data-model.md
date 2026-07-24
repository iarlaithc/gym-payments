# Data Model

## Entities

### Member
- Id (Guid)
- Name
- Email
- StripeCustomerId       // links to stripes customer object
- CreatedAt

### Membership
- Id (Guid)
- MemberId (FK)
- StripeSubscriptionId   // links to stripes subscription object
- Status                 // active, past_due, canceled
- CurrentPeriodEnd       // fetched from stripes Subscription object via SubscriptionService.GetAsync()
- CreatedAt

### Payment
- Id (Guid)
- MemberId (FK, nullable)
- StripePaymentIntentId  // for one-off "drop-in" payments
- StripeInvoiceId        // for subscription renewal / failed-payment events, nullable
- Amount                 // stored in cents (long), matches stripes own convention
- Currency
- Status                 // succeeded, failed, refunded
- Type                   // "subscription" or "drop_in"
- CreatedAt

### ProcessedWebhookEvent
- StripeEventId (string, primary key)  // Stripe's event.Id, e.g. evt_xxx
- ProcessedAt

## Known gaps / follow-up

- `Membership` currently assumes one active membership. wouldn't hold up if a member could pause and restart a subscription, since old and new Membership rows would need to be connected or disambiguated