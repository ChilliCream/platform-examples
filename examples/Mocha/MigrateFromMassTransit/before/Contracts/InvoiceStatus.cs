namespace Shop.Contracts;

public record InvoiceStatus(Guid OrderId, string Status);
