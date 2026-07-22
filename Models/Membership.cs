namespace GymPayments.Models;

public class Membership
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Member? Member { get; set; }
    public string StripeSubscriptionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // active, past_due, canceled
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}