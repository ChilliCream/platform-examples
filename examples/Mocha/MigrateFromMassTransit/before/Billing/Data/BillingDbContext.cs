using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Shop.Billing;

/// <summary>
/// Stores invoices and messaging state for the Billing service.
/// </summary>
public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.Entity<Invoice>().HasIndex(x => x.OrderId).IsUnique();
    }
}
