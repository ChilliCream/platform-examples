using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Creates an invoice for <see cref="OrderPlaced"/> and publishes confirmation when requested.
/// </summary>
public static class OrderPlacedHandler
{
    // The returned message cascades after Wolverine saves the invoice.
    public static InvoiceConfirmed? Handle(OrderPlaced message, BillingDb db, ILogger<OrderPlaced> logger)
    {
        db.Invoices.Add(new Invoice { OrderId = message.OrderId });
        Logs.InvoiceCreated(logger, message.OrderId);

        if (message.ConfirmInvoice)
        {
            return new InvoiceConfirmed(message.OrderId);
        }

        return null;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Invoice created for order {OrderId}")]
    public static partial void InvoiceCreated(ILogger logger, Guid orderId);
}
