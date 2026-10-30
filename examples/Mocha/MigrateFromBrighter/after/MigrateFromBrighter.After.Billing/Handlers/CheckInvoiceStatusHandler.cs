using Microsoft.EntityFrameworkCore;
using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class CheckInvoiceStatusHandler(BillingDbContext db)
    : IEventRequestHandler<CheckInvoiceStatus, InvoiceStatus>
{
    public async ValueTask<InvoiceStatus> HandleAsync(CheckInvoiceStatus message, CancellationToken cancellationToken)
    {
        var invoice = await db.Invoices.FindAsync([message.OrderId], cancellationToken);
        var status = invoice?.Status ?? "NotFound";
        return new InvoiceStatus { OrderId = message.OrderId, Status = status };
    }
}
