namespace Shop.Contracts;

/// <summary>
/// Requests scheduling of an order expiry.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="ExpiresAt">The time when the order expires.</param>
public sealed record ScheduleOrderExpiry(Guid OrderId, DateTimeOffset ExpiresAt);
