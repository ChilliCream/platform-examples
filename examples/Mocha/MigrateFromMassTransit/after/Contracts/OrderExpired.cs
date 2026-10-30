using Mocha.Sagas;

namespace Shop.Contracts;

public record OrderExpired(Guid OrderId) : ICorrelatable
{
    Guid? ICorrelatable.CorrelationId => OrderId;
}
