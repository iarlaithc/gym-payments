Architecture Decision Records

## 2026-07-22 - storing stripe ids and not payment details
stripe's Checkout handles all card entry, DB only stores references plus my busniess state.
good - we dont have to handle customer card data at all

## 2026-07-22 - Using SQlite for local dev with EF Core for abstraction
mostly for speed and familiarity, ef core lets us swap later if wanted 

## 2026-07-22 - Using long and cents instead of decimal and euros in Payment model
this matches what stripe have in their docs
