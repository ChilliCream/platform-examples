using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Requests expiry of an unconfirmed order.
/// </summary>
public sealed record ExpireOrder : ICorrelatable
{
    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
    /// <summary>
    /// Gets the order identifier used to correlate the message.
    /// </summary>
    Guid? ICorrelatable.CorrelationId => OrderId;
}
