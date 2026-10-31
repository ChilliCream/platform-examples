using Shop.Contracts;
using Wolverine.EntityFrameworkCore;

namespace Shop.Ordering;

public record PlaceOrderCommand(string Product, bool ConfirmInvoice = true);

public static class PlaceOrderCommandHandler
{
    // The order and outgoing events are saved together through Wolverine's EF outbox.
    public static async Task<Guid> Handle(
        PlaceOrderCommand command,
        IDbContextOutbox<OrderingDb> outbox,
        ILogger<PlaceOrderCommand> logger)
    {
        var id = Guid.NewGuid();
        outbox.DbContext.Orders.Add(new Order { Id = id, Product = command.Product });

        await outbox.PublishAsync(new OrderSubmitted(id));
        await outbox.PublishAsync(new OrderPlaced(id, command.Product, command.ConfirmInvoice));
        await outbox.SaveChangesAndFlushMessagesAsync();

        Logs.OrderSubmitted(logger, id);
        return id;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} submitted")]
    public static partial void OrderSubmitted(ILogger logger, Guid orderId);
}
