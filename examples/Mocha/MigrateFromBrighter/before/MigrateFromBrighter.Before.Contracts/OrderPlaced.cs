using Paramore.Brighter;

namespace Shop.Contracts;

public sealed class OrderPlaced : Event
{
    public OrderPlaced() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
    public string Product { get; set; } = string.Empty;
    public bool ConfirmInvoice { get; set; }
}
