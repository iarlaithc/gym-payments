namespace GymPayments.Models;

public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StripeCustomerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Membership> Memberships { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
}