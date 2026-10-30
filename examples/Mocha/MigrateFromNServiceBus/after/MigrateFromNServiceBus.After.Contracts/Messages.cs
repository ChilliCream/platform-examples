using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

public record OrderPlaced(Guid OrderId, bool ConfirmInvoice);
public record InvoiceConfirmed(Guid OrderId) : ICorrelatable
{
    Guid? ICorrelatable.CorrelationId => OrderId;
}
public record CheckInvoiceStatus(Guid OrderId) : IEventRequest<InvoiceStatus>;
public record InvoiceStatus(Guid OrderId, string Status);
public record SendReminder(Guid OrderId);
public record TryTransient(Guid Id);
public record TryInvalid(Guid Id);
