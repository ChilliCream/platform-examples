using Mocha.Sagas;

namespace Shop.Contracts;

public record InvoiceConfirmed(Guid OrderId) : ICorrelatable
{
    Guid? ICorrelatable.CorrelationId => OrderId;
}
