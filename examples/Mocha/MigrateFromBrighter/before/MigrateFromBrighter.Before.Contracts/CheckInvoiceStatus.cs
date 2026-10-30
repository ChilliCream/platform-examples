using Paramore.Brighter;

namespace Shop.Contracts;

public sealed class CheckInvoiceStatus : Request
{
    public CheckInvoiceStatus() : base(new ReplyAddress(string.Empty, string.Empty)) { }

    public CheckInvoiceStatus(ReplyAddress address) : base(address) { }

    public Guid OrderId { get; set; }
}
