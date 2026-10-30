namespace Shop.Contracts;

public record OrderPlaced(Guid OrderId, string Product, bool ConfirmInvoice);
public record InvoiceConfirmed(Guid Id);
public record CheckInvoiceStatus(Guid OrderId);
public record InvoiceStatus(Guid OrderId, string Status);
public record PaymentReminder(Guid OrderId);
public record AuditOrder(Guid OrderId);
public record FailOrder(Guid OrderId, string Failure);
