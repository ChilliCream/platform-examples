namespace Shop.Contracts;

/// <summary>
/// Requests delivery of a reminder for an order.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record SendReminder(Guid OrderId);
