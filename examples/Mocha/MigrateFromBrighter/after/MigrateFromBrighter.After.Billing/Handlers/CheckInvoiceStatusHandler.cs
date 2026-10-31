using Microsoft.EntityFrameworkCore;
using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Returns the invoice status for a <see cref="CheckInvoiceStatus"/> request.
/// </summary>
public sealed class CheckInvoiceStatusHandler(BillingDbContext db)
    : IEventRequestHandler<CheckInvoiceStatus, InvoiceStatus>
{
    public async ValueTask<InvoiceStatus> HandleAsync(CheckInvoiceStatus message, CancellationToken cancellationToken)
    {
        // Return the invoice status to the requesting service.
        var invoice = await db.Invoices.FindAsync([message.OrderId], cancellationToken);
        var status = invoice?.Status ?? "NotFound";
        return new InvoiceStatus
        {
            OrderId = message.OrderId,
            Status = status
        };
    }
}
