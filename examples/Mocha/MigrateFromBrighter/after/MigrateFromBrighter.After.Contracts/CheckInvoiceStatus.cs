using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Requests the invoice status for an order.
/// </summary>
public sealed record CheckInvoiceStatus : IEventRequest<InvoiceStatus>
{
    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
}
