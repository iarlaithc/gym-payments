namespace GymPayments.Models;

public class Payment
{
    public Guid Id { get; set; }
    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeInvoiceId { get; set; }
    public long Amount { get; set; }          // stored in cents, matches stripe 
    public string Currency { get; set; } = "eur";
    public string Status { get; set; } = string.Empty;   // succeeded, failed, refunded
    public string Type { get; set; } = string.Empty;     // "subscription" or "drop_in"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}