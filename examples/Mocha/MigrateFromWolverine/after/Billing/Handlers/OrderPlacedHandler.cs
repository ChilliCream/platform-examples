using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Creates an invoice for <see cref="OrderPlaced"/> and publishes confirmation when requested.
/// </summary>
public class OrderPlacedHandler(BillingDb db, IMessageBus bus, ILogger<OrderPlaced> logger) : IEventHandler<OrderPlaced>
{
    // The outbox publishes the confirmation after the invoice transaction commits.
    public async ValueTask HandleAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        db.Invoices.Add(new Invoice { OrderId = message.OrderId });
        Logs.InvoiceCreated(logger, message.OrderId);

        if (message.ConfirmInvoice)
        {
            await bus.PublishAsync(new InvoiceConfirmed(message.OrderId), cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Invoice created for order {OrderId}")]
    public static partial void InvoiceCreated(ILogger logger, Guid orderId);
}
