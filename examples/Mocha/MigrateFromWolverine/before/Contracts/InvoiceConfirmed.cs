namespace Shop.Contracts;

/// <summary>
/// Signals that an invoice has been confirmed for an order.
/// </summary>
/// <param name="Id">The order identifier used to correlate the saga.</param>
public sealed record InvoiceConfirmed(Guid Id);
