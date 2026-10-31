namespace Shop.Contracts;

/// <summary>
/// Requests a payment reminder for an order.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record PaymentReminder(Guid OrderId);
