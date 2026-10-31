namespace Shop.Ordering;

public record OrderSubmitted(Guid Id, string Product, bool ConfirmInvoice);
