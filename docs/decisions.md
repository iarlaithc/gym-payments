Architecture Decision Records

## 2026-07-22 - storing stripe ids and not payment details
stripe's Checkout handles all card entry, DB only stores references plus my busniess state.
good - we dont have to handle customer card data at all

## 2026-07-22 - Using SQlite for local dev with EF Core for abstraction
mostly for speed and familiarity, ef core lets us swap later if wanted 

## 2026-07-22 - Using long and cents instead of decimal and euros in Payment model
this matches what stripe have in their docs

## 2026-07-22 - stripe sheckout Sessions instead of custom payment forms
used stripes hosted Checkout page rather than building a custom card form, at the cost of less control over the payment page's styling. 
reasonable tradeoff for a project this size, less touching of customer data also.

## 2026-07-23 - some gaps in webhook testing data in db 
StripePaymentIntentId and StripeInvoiceId are blank, due to checkout session in subscription mode
the charge reference lives in the invoice to be handled later, unlike one off payments