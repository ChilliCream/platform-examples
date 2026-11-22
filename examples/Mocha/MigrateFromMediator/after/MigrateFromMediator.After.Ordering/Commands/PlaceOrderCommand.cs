using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

/// <summary>
/// Requests placement of an order and returns its identifier.
/// </summary>
/// <param name="ProductId">The identifier of the product to order.</param>
/// <param name="Quantity">The number of units to order.</param>
public sealed record PlaceOrderCommand(Guid ProductId, int Quantity) : ICommand<Guid>, IOrderCommand
{
    public string? Validate()
    {
        if (ProductId == Guid.Empty)
        {
            return "ProductId is required.";
        }

        if (Quantity <= 0)
        {
            return "Quantity must be positive.";
        }

        return null;
    }
}

/// <summary>
/// Saves an order and publishes an <see cref="OrderPlacedNotification"/>.
/// </summary>
public sealed class PlaceOrderHandler(IDbContextFactory<OrderDbContext> factory, IPublisher publisher)
    : ICommandHandler<PlaceOrderCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(PlaceOrderCommand message, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductId = message.ProductId,
            Quantity = message.Quantity
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        // Publish the notification after saving the order.
        await publisher.PublishAsync(
            new OrderPlacedNotification(order.Id, order.ProductId, order.Quantity),
            cancellationToken);

        return order.Id;
    }
}
