## Reminder for commands for restarting testing sessions
1. shh in
2. dotnet run
3. sstripe listen --forward-to localhost:PORT/webhook
 - take new whsec_ values and place into secrets
 - dotnet user-secrets set "Stripe:WebhookSecret" "whsec_xx"
4. curl -X POST "http://localhost:5228/checkout/membership?email=test3@example.com"