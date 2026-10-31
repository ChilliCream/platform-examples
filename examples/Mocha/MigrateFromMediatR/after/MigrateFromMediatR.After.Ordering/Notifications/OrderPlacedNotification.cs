using Mocha.Mediator;

namespace Shop.Ordering;

public sealed record OrderPlacedNotification(Guid OrderId, Guid ProductId, int Quantity) : INotification;

// Each notification handler receives the same published event.
public sealed class OrderReceiptHandler(ILogger<OrderReceiptHandler> logger)
    : INotificationHandler<OrderPlacedNotification>
{
    public ValueTask HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        Logs.ReceiptPrepared(logger, notification.OrderId);
        return ValueTask.CompletedTask;
    }
}

// Each notification handler receives the same published event.
public sealed class OrderInventoryHandler(ILogger<OrderInventoryHandler> logger)
    : INotificationHandler<OrderPlacedNotification>
{
    public ValueTask HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        Logs.InventoryNotified(logger, notification.OrderId);
        return ValueTask.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Receipt prepared for order {OrderId}")]
    public static partial void ReceiptPrepared(ILogger logger, Guid orderId);

    [LoggerMessage(LogLevel.Information, "Inventory notified for order {OrderId}")]
    public static partial void InventoryNotified(ILogger logger, Guid orderId);
}
