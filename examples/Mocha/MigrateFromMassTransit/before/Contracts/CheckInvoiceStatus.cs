namespace Shop.Contracts;

/// <summary>
/// Requests the status of an order’s invoice.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record CheckInvoiceStatus(Guid OrderId);
