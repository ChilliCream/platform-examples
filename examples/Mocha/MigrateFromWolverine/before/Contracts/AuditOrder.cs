namespace Shop.Contracts;

/// <summary>
/// Requests an audit of an order.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record AuditOrder(Guid OrderId);
