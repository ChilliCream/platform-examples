namespace Shop.Contracts;

public record ProcessInvoice(Guid OrderId, string Failure);
