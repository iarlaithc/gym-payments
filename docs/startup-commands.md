## Reminder for commands for restarting testing sessions
1. shh in
2. dotnet run
3. sstripe listen --forward-to localhost:PORT/webhook
 - take new whsec_ values and place into secrets
 - dotnet user-secrets set "Stripe:WebhookSecret" "whsec_98c93100c1401dd01f8aa0176583387eb15cbc389627b5b1b6059e31472d47bf"
4. curl -X POST "http://localhost:5228/checkout/membership?email=test3@example.com"