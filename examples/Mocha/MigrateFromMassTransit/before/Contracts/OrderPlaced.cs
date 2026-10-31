namespace Shop.Contracts;

public record OrderPlaced(Guid OrderId, decimal Amount, bool ConfirmInvoice);
