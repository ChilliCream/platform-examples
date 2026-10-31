using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Contains the invoice status returned for an order.
/// </summary>
public sealed record InvoiceStatus
{
    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
    /// <summary>
    /// Gets or sets the invoice status, or <c>NotFound</c> when no invoice exists.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
