using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

/// <summary>
/// Provides access to stored <see cref="Order"/> entities.
/// </summary>
public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}
