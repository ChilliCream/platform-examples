using Bookshop.Domain.Orders;

namespace Bookshop.Application.Tests.Orders.Fakes;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _orders = [];

    public Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_orders.GetValueOrDefault(id));

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
}
