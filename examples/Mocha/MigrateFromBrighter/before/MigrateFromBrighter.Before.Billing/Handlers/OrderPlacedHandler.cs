using Microsoft.EntityFrameworkCore;
using Npgsql;
using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class OrderPlacedHandler(
    BillingDbContext db,
    IAmACommandProcessor processor,
    ILogger<OrderPlacedHandler> logger)
    : RequestHandlerAsync<OrderPlaced>
{
    public override async Task<OrderPlaced> HandleAsync(
        OrderPlaced message,
        CancellationToken cancellationToken = default)
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
            await processor.PostAsync(new InvoiceConfirmed { OrderId = message.OrderId }, cancellationToken: cancellationToken);
        }
        return await base.HandleAsync(message, cancellationToken);
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Invoice created for order {OrderId}")]
    public static partial void InvoiceCreated(ILogger logger, Guid orderId);
}
