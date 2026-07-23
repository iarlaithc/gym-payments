namespace GymPayments;
public class ProcessedWebhookEvent
{
    public string StripeEventId { get; set; } = string.Empty; //pk
    public DateTime ProcessedAt {get; set; } = DateTime.UtcNow;
    
}