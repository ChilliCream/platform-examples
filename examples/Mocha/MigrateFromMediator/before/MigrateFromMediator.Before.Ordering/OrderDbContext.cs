using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}

public sealed class Order
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = "Placed";
}
