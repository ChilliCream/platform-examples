using Microsoft.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Outbox;
using Mocha.Sagas.EfCore;
using Mocha.Scheduling;

namespace Shop.Ordering;

/// <summary>
/// Stores orders for the Ordering service.
/// </summary>
public sealed class OrderingDb(DbContextOptions<OrderingDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddPostgresOutbox();
        modelBuilder.AddPostgresInbox();
        modelBuilder.AddPostgresSagas();
        modelBuilder.AddPostgresScheduledMessages();
    }
}
