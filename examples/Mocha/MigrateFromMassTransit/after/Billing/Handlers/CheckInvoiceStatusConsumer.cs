using Mocha;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class CheckInvoiceStatusConsumer(BillingDbContext db) : IEventRequestHandler<CheckInvoiceStatus, InvoiceStatus>
{
    public async ValueTask<InvoiceStatus> HandleAsync(CheckInvoiceStatus message, CancellationToken cancellationToken)
    {
        // Return the invoice status to the requesting service.
        var invoice = await db.Invoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrderId == message.OrderId, cancellationToken);
        return new InvoiceStatus(message.OrderId, invoice?.Status ?? "NotFound");
    }
}
