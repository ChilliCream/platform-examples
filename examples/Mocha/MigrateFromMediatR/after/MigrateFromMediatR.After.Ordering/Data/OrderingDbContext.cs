using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

/// <summary>
/// Stores <see cref="Order"/> entities.
/// </summary>
public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}
