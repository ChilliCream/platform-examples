using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Returns the <see cref="InvoiceStatus"/> for a <see cref="CheckInvoiceStatus"/> request.
/// </summary>
public static class CheckInvoiceStatusHandler
{
    // Wolverine sends the return value back to the requesting service.
    public static async Task<InvoiceStatus> Handle(CheckInvoiceStatus message, BillingDb db)
    {
        var invoice = await db.Invoices.FindAsync(message.OrderId);
        return new InvoiceStatus(message.OrderId, invoice?.Status ?? "NotFound");
    }
}
