namespace Shop.Contracts;

/// <summary>
/// Contains the invoice status returned for an order.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Status">The invoice status, or NotFound if no invoice exists.</param>
public sealed record InvoiceStatus(Guid OrderId, string Status);
