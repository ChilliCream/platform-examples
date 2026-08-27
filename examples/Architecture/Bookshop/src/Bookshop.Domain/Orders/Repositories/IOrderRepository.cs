namespace Bookshop.Domain.Orders;

/// <summary>
/// Loads and stores <see cref="Order"/> aggregate roots as whole units.
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}
