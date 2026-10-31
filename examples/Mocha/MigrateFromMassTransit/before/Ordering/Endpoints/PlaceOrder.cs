namespace Shop.Ordering;

/// <summary>
/// Specifies an order to create through the Ordering API.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Amount">The order amount.</param>
/// <param name="ConfirmInvoice">Whether Billing should schedule an invoice confirmation.</param>
public sealed record PlaceOrder(Guid OrderId, decimal Amount, bool ConfirmInvoice);
