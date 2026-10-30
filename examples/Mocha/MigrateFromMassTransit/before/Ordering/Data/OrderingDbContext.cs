using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : SagaDbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderState> OrderStates => Set<OrderState>();

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new OrderStateMap(); }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
