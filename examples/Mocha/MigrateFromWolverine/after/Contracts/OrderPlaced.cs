namespace Shop.Contracts;

/// <summary>
/// Signals that an order is ready for invoicing.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Product">The product ordered.</param>
/// <param name="ConfirmInvoice">Whether Billing should confirm the invoice.</param>
public sealed record OrderPlaced(Guid OrderId, string Product, bool ConfirmInvoice);
