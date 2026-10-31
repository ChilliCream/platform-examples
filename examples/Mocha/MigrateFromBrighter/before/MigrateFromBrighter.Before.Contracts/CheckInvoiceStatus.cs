using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Requests the invoice status for an order.
/// </summary>
public sealed class CheckInvoiceStatus : Request
{
    public CheckInvoiceStatus() : base(new ReplyAddress(string.Empty, string.Empty)) { }

    public CheckInvoiceStatus(ReplyAddress address) : base(address) { }

    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
}
