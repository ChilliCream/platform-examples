using Paramore.Brighter;

namespace Shop.Contracts;

public sealed class InvoiceConfirmed : Event
{
    public InvoiceConfirmed() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}
