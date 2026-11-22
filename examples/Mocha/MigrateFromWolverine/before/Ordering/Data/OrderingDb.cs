using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

/// <summary>
/// Stores orders for the Ordering service.
/// </summary>
public sealed class OrderingDb(DbContextOptions<OrderingDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderSaga> Sagas => Set<OrderSaga>();
}
