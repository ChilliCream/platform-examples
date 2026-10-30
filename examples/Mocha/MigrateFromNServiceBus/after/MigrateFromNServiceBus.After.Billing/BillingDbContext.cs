using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Mocha.Inbox;
using Mocha.Outbox;
using Mocha.Scheduling;

namespace Shop.Billing;

public class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddPostgresOutbox();
        modelBuilder.AddPostgresInbox();
        modelBuilder.AddPostgresScheduledMessages();
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");
            entity.HasKey(x => x.OrderId);
            entity.Property(x => x.OrderId).HasColumnName("order_id");
            entity.Property(x => x.Status).HasColumnName("status");
        });
    }
}

public class BillingDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseNpgsql("Host=localhost;Database=billing-db;Username=postgres;Password=postgres")
            .Options;
        return new BillingDbContext(options);
    }
}

public class Invoice
{
    public Guid OrderId { get; set; }
    public required string Status { get; set; }
}
