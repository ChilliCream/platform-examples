using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}
