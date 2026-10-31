using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<OrderDto?>;

public sealed record OrderDto(Guid OrderId, Guid ProductId, int Quantity, string Status);

// Queries read without tracking because they do not change the order.
public sealed class GetOrderHandler(IDbContextFactory<OrderDbContext> factory)
    : IRequestHandler<GetOrderQuery, OrderDto?>
{
    public async ValueTask<OrderDto?> Handle(GetOrderQuery message, CancellationToken cancellationToken)
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
