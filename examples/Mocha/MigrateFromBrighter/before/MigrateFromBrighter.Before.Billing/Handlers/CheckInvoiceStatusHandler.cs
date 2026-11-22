using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Returns the invoice status for a <see cref="CheckInvoiceStatus"/> request.
/// </summary>
public sealed class CheckInvoiceStatusHandler(BillingDbContext db, IAmACommandProcessor processor)
    : RequestHandlerAsync<CheckInvoiceStatus>
{
    public override async Task<CheckInvoiceStatus> HandleAsync(
        CheckInvoiceStatus message, CancellationToken cancellationToken = default)
    {
        // Send the invoice status to the request's reply address.
        var invoice = await db.Invoices.FindAsync([message.OrderId], cancellationToken);
        var status = invoice?.Status ?? "NotFound";
        await processor.PostAsync(
            new InvoiceStatus(message.ReplyAddress)
            {
                OrderId = message.OrderId,
                Status = status
            },
            cancellationToken: cancellationToken);

        return await base.HandleAsync(message, cancellationToken);
    }
}
