using Microsoft.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Outbox;

namespace Shop.Billing;

/// <summary>
/// Stores invoices for the Billing service.
/// </summary>
public sealed class BillingDb(DbContextOptions<BillingDb> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>().HasKey(x => x.OrderId);
        modelBuilder.AddPostgresOutbox();
        modelBuilder.AddPostgresInbox();
    }
}
