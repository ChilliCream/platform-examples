using Mocha.Sagas;

namespace Shop.Contracts;

public record InvoiceConfirmed(Guid Id) : ICorrelatable
{
    Guid? ICorrelatable.CorrelationId => Id;
}
