using Microsoft.EntityFrameworkCore;
using Mocha.Outbox;
using Mocha.Sagas.EfCore;
using Mocha.Scheduling;

namespace Shop.Ordering;

/// <summary>
/// Stores orders, order process statuses and reminders.
/// </summary>
public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderProcess> OrderProcesses => Set<OrderProcess>();
    public DbSet<Reminder> Reminders => Set<Reminder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id")
                .ValueGeneratedNever();
            entity.Property(x => x.Product).HasColumnName("product");
        });
        modelBuilder.Entity<OrderProcess>(entity =>
        {
            entity.ToTable("order_processes");
            entity.HasKey(x => x.OrderId);
            entity.Property(x => x.OrderId).HasColumnName("order_id")
                .ValueGeneratedNever();
            entity.Property(x => x.Status).HasColumnName("status");
            entity.HasOne<Order>()
                .WithOne()
                .HasForeignKey<OrderProcess>(x => x.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.ToTable("reminders");
            entity.HasKey(x => x.OrderId);
            entity.Property(x => x.OrderId).HasColumnName("order_id")
                .ValueGeneratedNever();
            entity.HasOne<Order>()
                .WithOne()
                .HasForeignKey<Reminder>(x => x.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.AddPostgresOutbox();
        modelBuilder.AddPostgresSagas();
        modelBuilder.AddPostgresScheduledMessages();
    }
}
