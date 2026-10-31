using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

public sealed class OrderingDb(DbContextOptions<OrderingDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderSaga> Sagas => Set<OrderSaga>();
}
