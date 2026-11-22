namespace Shop.Contracts;

/// <summary>
/// Requests a simulated processing failure for an order.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Failure">The failure to simulate, either validation or transient.</param>
public sealed record FailOrder(Guid OrderId, string Failure);
