namespace Shop.Ordering;

/// <summary>
/// Starts tracking an order before publishing its placement.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Amount">The order amount.</param>
/// <param name="ConfirmInvoice">Whether Billing should schedule an invoice confirmation.</param>
public sealed record StartOrder(Guid OrderId, decimal Amount, bool ConfirmInvoice);
