using Microsoft.EntityFrameworkCore;
using GymPayments.Models;

namespace GymPayments.Data;

public class GymPaymentsContext : DbContext
{
    public GymPaymentsContext(DbContextOptions<GymPaymentsContext> options) : base(options) {}
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Payment> Payments => Set<Payment>();
}