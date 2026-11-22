using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Signals that an order’s invoice has been confirmed.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record InvoiceConfirmed(Guid OrderId) : ICorrelatable
{
    /// <summary>
    /// Gets the order identifier used to correlate the message.
    /// </summary>
    Guid? ICorrelatable.CorrelationId => OrderId;
}
