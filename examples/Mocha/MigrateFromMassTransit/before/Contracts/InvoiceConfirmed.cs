namespace Shop.Contracts;

/// <summary>
/// Signals that an order’s invoice has been confirmed.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record InvoiceConfirmed(Guid OrderId);
