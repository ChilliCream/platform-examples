using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Signals that an order’s confirmation period has expired.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record OrderExpired(Guid OrderId) : ICorrelatable
{
    /// <summary>
    /// Gets the order identifier used to correlate the message.
    /// </summary>
    Guid? ICorrelatable.CorrelationId => OrderId;
}
