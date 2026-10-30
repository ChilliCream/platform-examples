using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Ordering;

// Brighter dispatches the RPC reply to this handler before returning it to the caller.
public sealed class InvoiceStatusHandler : RequestHandler<InvoiceStatus>
{
    public override InvoiceStatus Handle(InvoiceStatus message) => base.Handle(message);
}
