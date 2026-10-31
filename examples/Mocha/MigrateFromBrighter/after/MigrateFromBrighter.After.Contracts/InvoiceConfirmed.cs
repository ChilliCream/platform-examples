using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Reports that an invoice has been confirmed for an order.
/// </summary>
public sealed record InvoiceConfirmed : ICorrelatable
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
