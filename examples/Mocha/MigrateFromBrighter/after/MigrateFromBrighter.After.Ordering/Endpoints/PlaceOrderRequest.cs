namespace Shop.Ordering;

public sealed record PlaceOrderRequest(string Product, bool ConfirmInvoice);
