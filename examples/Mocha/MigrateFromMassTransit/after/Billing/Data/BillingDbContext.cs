using Microsoft.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Outbox;
using Mocha.Scheduling;

namespace Shop.Billing;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddPostgresInbox();
        modelBuilder.AddPostgresOutbox();
        modelBuilder.AddPostgresScheduledMessages();
        modelBuilder.Entity<Invoice>().HasIndex(x => x.OrderId).IsUnique();
    }
}
