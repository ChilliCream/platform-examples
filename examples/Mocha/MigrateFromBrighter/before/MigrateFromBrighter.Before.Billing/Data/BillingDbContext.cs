using Microsoft.EntityFrameworkCore;

namespace Shop.Billing;

/// <summary>
/// Stores billing invoices and simulated failure attempts.
/// </summary>
public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Attempt> Attempts => Set<Attempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");
            entity.HasKey(x => x.OrderId);
            entity.Property(x => x.OrderId).HasColumnName("order_id")
                .ValueGeneratedNever();
            entity.Property(x => x.Status).HasColumnName("status");
        });
        modelBuilder.Entity<Attempt>(entity =>
        {
            entity.ToTable("attempts");
            entity.HasKey(x => x.OrderId);
            entity.Property(x => x.OrderId).HasColumnName("order_id")
                .ValueGeneratedNever();
            entity.Property(x => x.Count).HasColumnName("count");
        });
    }
}
