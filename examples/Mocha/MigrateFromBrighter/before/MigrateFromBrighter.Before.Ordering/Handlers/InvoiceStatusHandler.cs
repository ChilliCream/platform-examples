using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Handles invoice status replies dispatched by Brighter before they are returned to the caller.
/// </summary>
public sealed class InvoiceStatusHandler : RequestHandler<InvoiceStatus>
{
    public override InvoiceStatus Handle(InvoiceStatus message) => base.Handle(message);
}
