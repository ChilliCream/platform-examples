namespace Shop.Ordering;

public record PlaceOrder(Guid OrderId, decimal Amount, bool ConfirmInvoice);
