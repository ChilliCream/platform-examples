using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Contains the invoice status returned for an order.
/// </summary>
public sealed class InvoiceStatus : Reply
{
    public InvoiceStatus(ReplyAddress address) : base(address) { }

    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
    /// <summary>
    /// Gets or sets the invoice status, or <c>NotFound</c> when no invoice exists.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
