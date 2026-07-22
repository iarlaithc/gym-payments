Entities

Member
- Id (Guid)
- Name
- Email
- StripeCustomerId       // links to Stripe's Customer object
- CreatedAt

Membership
- Id (Guid)
- MemberId (FK)
- StripeSubscriptionId   // links to Stripe's Subscription object
- Status                 // active, past_due, canceled. mirroring Stripe's subscription status
- CurrentPeriodEnd
- CreatedAt

Payment
- Id (Guid)
- MemberId (FK, nullable -> a drop-in payer might not be a full Member)
- StripePaymentIntentId  // for one-off "drop in" payments
- StripeInvoiceId        // for subscription renewal payments, nullable
- Amount
- Currency
- Status                 // succeeded, failed, refunded
- Type                   // "subscription" or "drop_in"
- CreatedAt