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

public sealed class OrderStatus
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
}

public sealed class Order
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public bool ConfirmInvoice { get; set; }
}
