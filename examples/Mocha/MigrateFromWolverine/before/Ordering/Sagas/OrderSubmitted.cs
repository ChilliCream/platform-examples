namespace Shop.Ordering;

/// <summary>
/// Starts the saga for a submitted order.
/// </summary>
/// <param name="Id">The order identifier used to correlate the saga.</param>
public sealed record OrderSubmitted(Guid Id);
