using Microsoft.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Outbox;
using Mocha.Sagas.EfCore;
using Mocha.Scheduling;

namespace Shop.Ordering;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatus> OrderStates => Set<OrderStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddPostgresOutbox();
        modelBuilder.AddPostgresInbox();
        modelBuilder.AddPostgresSagas();
        modelBuilder.AddPostgresScheduledMessages();
        modelBuilder.Entity<OrderStatus>().HasKey(x => x.CorrelationId);
    }
}
