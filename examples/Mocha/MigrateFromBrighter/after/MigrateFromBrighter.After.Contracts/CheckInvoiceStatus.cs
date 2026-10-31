using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

public sealed class CheckInvoiceStatus : IEventRequest<InvoiceStatus>
{
    public Guid OrderId { get; set; }
}
