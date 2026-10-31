using Microsoft.Extensions.Options;
using Shop.Contracts;
using Wolverine;

namespace Shop.Ordering;

/// <summary>
/// Tracks an order until its invoice is confirmed or the saga times out, correlating messages by the order ID.
/// </summary>
public class OrderSaga : Saga
{
    public Guid Id { get; set; }

    public static (OrderSaga, OrderExpired) Start(
        OrderSubmitted message,
        IOptions<ScenarioOptions> options,
        ILogger<OrderSaga> logger)
    {
        Logs.SagaSubmitted(logger, message.Id);
        return (new OrderSaga { Id = message.Id },
            new OrderExpired(message.Id, TimeSpan.FromSeconds(options.Value.SagaTimeoutSeconds)));
    }

    public async Task Handle(InvoiceConfirmed message, OrderingDb db, ILogger<OrderSaga> logger)
    {
        var order = await db.Orders.FindAsync(Id);
        if (order is not null)
        {
            order.Status = "Accepted";
        }

        Logs.OrderAccepted(logger, Id);
        MarkCompleted();
    }

    public async Task Handle(OrderExpired message, OrderingDb db, ILogger<OrderSaga> logger)
    {
        var order = await db.Orders.FindAsync(Id);
        if (order is not null)
        {
            order.Status = "Expired";
        }

        Logs.OrderExpired(logger, Id);
        MarkCompleted();
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} saga submitted")]
    public static partial void SagaSubmitted(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} accepted")]
    public static partial void OrderAccepted(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} expired")]
    public static partial void OrderExpired(ILogger logger, Guid orderId);
}
