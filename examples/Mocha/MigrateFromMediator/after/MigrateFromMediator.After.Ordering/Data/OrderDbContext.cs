using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}
