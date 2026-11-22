namespace Shop.Ordering;

/// <summary>
/// Contains the details submitted to place an order.
/// </summary>
/// <param name="Product">The product to order.</param>
/// <param name="ConfirmInvoice">Whether Billing should confirm the invoice.</param>
public sealed record PlaceOrderRequest(string Product, bool ConfirmInvoice);
