using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

public sealed class InvoiceConfirmed : ICorrelatable
{
    public Guid OrderId { get; set; }
    Guid? ICorrelatable.CorrelationId => OrderId;
}
