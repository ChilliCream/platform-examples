using Microsoft.EntityFrameworkCore;
using Npgsql;
using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class OrderPlacedHandler(
    BillingDbContext db,
    IMessageBus bus,
    ILogger<OrderPlacedHandler> logger)
    : IEventHandler<OrderPlaced>
{
    public async ValueTask HandleAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        // A redelivered event must not create a second invoice.
        var invoice = await db.Invoices.FindAsync([message.OrderId], cancellationToken);
        if (invoice is null)
        {
            invoice = new Invoice
            {
                OrderId = message.OrderId,
                Status = "Created"
            };
            db.Invoices.Add(invoice);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                Logs.InvoiceCreated(logger, message.OrderId);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // A concurrent delivery already inserted this row.
                db.Entry(invoice).State = EntityState.Detached;
            }
        }

        if (message.ConfirmInvoice)
        {
            await bus.PublishAsync(new InvoiceConfirmed { OrderId = message.OrderId }, cancellationToken);
        }
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Invoice created for order {OrderId}")]
    public static partial void InvoiceCreated(ILogger logger, Guid orderId);
}
