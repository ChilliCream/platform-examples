using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Reports that an invoice has been confirmed for an order.
/// </summary>
public sealed class InvoiceConfirmed : Event
{
    public InvoiceConfirmed() : base(Id.Random()) { }

    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
}
