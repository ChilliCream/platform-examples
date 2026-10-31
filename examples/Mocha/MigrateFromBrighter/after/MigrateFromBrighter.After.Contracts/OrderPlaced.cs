using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Reports an order submitted for billing.
/// </summary>
public sealed record OrderPlaced
{
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
