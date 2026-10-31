using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Returns the <see cref="InvoiceStatus"/> for a <see cref="CheckInvoiceStatus"/> request.
/// </summary>
public class CheckInvoiceStatusHandler(BillingDb db) : IEventRequestHandler<CheckInvoiceStatus, InvoiceStatus>
{
    // Mocha sends the return value back to the requesting service.
    public async ValueTask<InvoiceStatus> HandleAsync(CheckInvoiceStatus message, CancellationToken cancellationToken)
    {
        var invoice = await db.Invoices.FindAsync([message.OrderId], cancellationToken);
        return new InvoiceStatus(message.OrderId, invoice?.Status ?? "NotFound");
    }
}
