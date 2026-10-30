namespace Shop.Contracts;

public record OrderPlaced(Guid OrderId, decimal Amount, bool ConfirmInvoice);
public record InvoiceConfirmed(Guid OrderId);
public record OrderExpired(Guid OrderId);
public record SendReminder(Guid OrderId);
public record CheckInvoiceStatus(Guid OrderId);
public record InvoiceStatus(Guid OrderId, string Status);
public record ProcessInvoice(Guid OrderId, string Failure);
