using Paramore.Brighter;

namespace Shop.Contracts;

public sealed class InvoiceStatus : Reply
{
    public InvoiceStatus(ReplyAddress address) : base(address) { }

    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
