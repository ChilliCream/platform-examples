using Mocha;
using Mocha.Mediator;

namespace Shop.Ordering;

public record PlaceOrderCommand(string Product, bool ConfirmInvoice = true) : ICommand<Guid>;

public class PlaceOrderCommandHandler(OrderingDb db, IMessageBus bus, ILogger<PlaceOrderCommand> logger)
    : ICommandHandler<PlaceOrderCommand, Guid>
{
    // The mediator transaction saves the order and outgoing event together in the EF outbox.
    public async ValueTask<Guid> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        db.Orders.Add(new Order { Id = id, Product = command.Product });

        await bus.PublishAsync(new OrderSubmitted(id, command.Product, command.ConfirmInvoice), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        Logs.OrderSubmitted(logger, id);
        return id;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} submitted")]
    public static partial void OrderSubmitted(ILogger logger, Guid orderId);
}
