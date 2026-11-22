using MediatR;

namespace Shop.Ordering;

/// <summary>
/// Announces that an <see cref="Order"/> has been saved.
/// </summary>
/// <param name="OrderId">The ID of the saved order.</param>
/// <param name="ProductId">The ID of the ordered product.</param>
/// <param name="Quantity">The number of units ordered.</param>
public sealed record OrderPlacedNotification(Guid OrderId, Guid ProductId, int Quantity) : INotification;

// Each notification handler receives the same published event.
/// <summary>
/// Logs receipt preparation for each <see cref="OrderPlacedNotification"/>.
/// </summary>
public sealed class OrderReceiptHandler(ILogger<OrderReceiptHandler> logger)
    : INotificationHandler<OrderPlacedNotification>
{
    public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        Logs.ReceiptPrepared(logger, notification.OrderId);
        return Task.CompletedTask;
    }
}

// Each notification handler receives the same published event.
/// <summary>
/// Logs inventory notification for each <see cref="OrderPlacedNotification"/>.
/// </summary>
public sealed class OrderInventoryHandler(ILogger<OrderInventoryHandler> logger)
    : INotificationHandler<OrderPlacedNotification>
{
    public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        Logs.InventoryNotified(logger, notification.OrderId);
        return Task.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Receipt prepared for order {OrderId}")]
    public static partial void ReceiptPrepared(ILogger logger, Guid orderId);

    [LoggerMessage(LogLevel.Information, "Inventory notified for order {OrderId}")]
    public static partial void InventoryNotified(ILogger logger, Guid orderId);
}
