namespace Shop.Contracts;

/// <summary>
/// Returns the status of an order’s invoice.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Status">The invoice status, or <c>NotFound</c> when no invoice exists.</param>
public sealed record InvoiceStatus(Guid OrderId, string Status);
