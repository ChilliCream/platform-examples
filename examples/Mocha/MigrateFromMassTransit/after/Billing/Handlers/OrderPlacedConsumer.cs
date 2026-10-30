using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class OrderPlacedConsumer(
    BillingDbContext db,
    IMessageBus bus,
    IConfiguration configuration,
    ILogger<OrderPlacedConsumer> logger) : IEventHandler<OrderPlaced>
{
    public async ValueTask HandleAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        db.Invoices.Add(new Invoice
        {
            Id = message.OrderId,
            OrderId = message.OrderId,
            Amount = message.Amount,
            Status = "Created"
        });
        await db.SaveChangesAsync(cancellationToken);
        Logs.InvoiceCreated(logger, message.OrderId);

        // Publish confirmation after a short delay so the saga can observe it.
        if (message.ConfirmInvoice)
        {
            var delay = configuration.GetValue<int>("Scenario:ConfirmationDelaySeconds");
            await bus.SchedulePublishAsync(
                new InvoiceConfirmed(message.OrderId),
                DateTimeOffset.UtcNow.AddSeconds(delay),
                cancellationToken);
        }
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Invoice for order {OrderId} created")]
    public static partial void InvoiceCreated(ILogger logger, Guid orderId);
}
