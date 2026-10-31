namespace Shop.Ordering;

/// <summary>
/// Starts the saga for a submitted order.
/// </summary>
/// <param name="Id">The order identifier used to correlate the saga.</param>
/// <param name="Product">The product ordered.</param>
/// <param name="ConfirmInvoice">Whether Billing should confirm the invoice.</param>
public sealed record OrderSubmitted(Guid Id, string Product, bool ConfirmInvoice);
