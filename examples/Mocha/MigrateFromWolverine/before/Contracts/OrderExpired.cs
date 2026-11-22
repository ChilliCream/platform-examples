using Wolverine;

namespace Shop.Ordering;

/// <summary>
/// Signals that the order saga timeout has elapsed.
/// </summary>
/// <param name="Id">The order identifier used to correlate the saga.</param>
/// <param name="Delay">The delay before the timeout is delivered.</param>
public sealed record OrderExpired(Guid Id, TimeSpan Delay) : TimeoutMessage(Delay);
