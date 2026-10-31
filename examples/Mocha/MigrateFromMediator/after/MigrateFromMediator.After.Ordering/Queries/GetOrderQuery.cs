using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

/// <summary>
/// Requests the details of an order.
/// </summary>
/// <param name="OrderId">The identifier of the order to retrieve.</param>
public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderDto?>;

/// <summary>
/// Contains the details of an order returned by a <see cref="GetOrderQuery"/>.
/// </summary>
/// <param name="OrderId">The identifier of the order.</param>
/// <param name="ProductId">The identifier of the ordered product.</param>
/// <param name="Quantity">The number of units ordered.</param>
/// <param name="Status">The current order status.</param>
public sealed record OrderDto(Guid OrderId, Guid ProductId, int Quantity, string Status);

// Queries read without tracking because they do not change the order.
/// <summary>
/// Returns the requested order, or null when it does not exist.
/// </summary>
public sealed class GetOrderHandler(IDbContextFactory<OrderDbContext> factory)
    : IQueryHandler<GetOrderQuery, OrderDto?>
{
    public async ValueTask<OrderDto?> HandleAsync(GetOrderQuery message, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var order = await db.Orders
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == message.OrderId, cancellationToken);

        if (order is null)
        {
            return null;
        }

        return new OrderDto(order.Id, order.ProductId, order.Quantity, order.Status);
    }
}
