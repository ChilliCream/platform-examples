using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Reports an order submitted for billing.
/// </summary>
public sealed class OrderPlaced : Event
{
    public OrderPlaced() : base(Id.Random()) { }

    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
    /// <summary>
    /// Gets or sets the product to order.
    /// </summary>
    public string Product { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets whether Billing should confirm the invoice.
    /// </summary>
    public bool ConfirmInvoice { get; set; }
}
