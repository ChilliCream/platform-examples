using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

public sealed class PlaceOrderHandler(IDbContextFactory<OrderDbContext> factory, IPublisher publisher)
    : ICommandHandler<PlaceOrder, Guid>
{
    public async ValueTask<Guid> HandleAsync(PlaceOrder message, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var order = new Order { Id = Guid.NewGuid(), ProductId = message.ProductId, Quantity = message.Quantity };
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        await publisher.PublishAsync(new OrderPlaced(order.Id, order.ProductId, order.Quantity), cancellationToken);
        return order.Id;
    }
}

public sealed class CancelOrderHandler(IDbContextFactory<OrderDbContext> factory) : ICommandHandler<CancelOrder>
{
    public async ValueTask HandleAsync(CancelOrder message, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var order = await db.Orders.FindAsync([message.OrderId], cancellationToken);
        if (order is null)
        {
            throw new OrderNotFoundException(message.OrderId);
        }

        order.Status = "Cancelled";
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetOrderHandler(IDbContextFactory<OrderDbContext> factory) : IQueryHandler<GetOrder, OrderDto?>
{
    public async ValueTask<OrderDto?> HandleAsync(GetOrder message, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var order = await db.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == message.OrderId, cancellationToken);
        return order is null ? null : new OrderDto(order.Id, order.ProductId, order.Quantity, order.Status);
    }
}

public sealed class OrderAuditHandler(ILogger<OrderAuditHandler> logger) : INotificationHandler<OrderPlaced>
{
    public ValueTask HandleAsync(OrderPlaced notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order audit recorded for {OrderId}", notification.OrderId);
        return ValueTask.CompletedTask;
    }
}

public sealed class OrderNotFoundException(Guid orderId) : Exception($"Order {orderId} was not found.");
