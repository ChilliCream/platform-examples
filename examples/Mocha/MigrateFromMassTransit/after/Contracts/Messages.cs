using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

public record OrderPlaced(Guid OrderId, decimal Amount, bool ConfirmInvoice);
public record InvoiceConfirmed(Guid OrderId) : ICorrelatable
{
    public Guid? CorrelationId => OrderId;
}
public record OrderExpired(Guid OrderId);
public record SendReminder(Guid OrderId);
public record CheckInvoiceStatus(Guid OrderId) : IEventRequest<InvoiceStatus>;
public record InvoiceStatus(Guid OrderId, string Status);
public record ProcessInvoice(Guid OrderId, string Failure);
