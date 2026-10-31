namespace Shop.Contracts;

/// <summary>
/// Signals that an order was placed.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Amount">The order amount.</param>
/// <param name="ConfirmInvoice">Whether Billing should schedule an invoice confirmation.</param>
public sealed record OrderPlaced(Guid OrderId, decimal Amount, bool ConfirmInvoice);
