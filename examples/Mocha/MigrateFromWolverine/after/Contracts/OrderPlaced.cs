namespace Shop.Contracts;

public record OrderPlaced(Guid OrderId, string Product, bool ConfirmInvoice);
