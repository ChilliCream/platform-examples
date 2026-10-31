namespace Shop.Contracts;

/// <summary>
/// Requests the <see cref="InvoiceStatus"/> for an order.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record CheckInvoiceStatus(Guid OrderId);
