using Bookshop.Domain.Orders;
using Bookshop.Infrastructure.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Orders.Persistence;

/// <summary>
/// Loads and stores <see cref="Order"/> aggregate roots as tracked entities. Changes are
/// persisted only when <see cref="Bookshop.Application.Common.IUnitOfWork.SaveChangesAsync"/>
/// is called; this repository never calls <c>SaveChanges</c> itself.
/// </summary>
internal sealed class OrderRepository(BookshopContext context) : IOrderRepository
{
    // Order.Lines is mapped as an owned collection (OwnsMany in BookshopContext), and EF Core
    // always loads owned entities together with their owner, so no explicit Include is needed
    // to bring the order lines along with the aggregate root.
    public Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Orders.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        context.Orders.Add(order);
        return Task.CompletedTask;
    }
}
