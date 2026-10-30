namespace Shop.Ordering;

public record StartOrder(Guid OrderId, decimal Amount, bool ConfirmInvoice);
