namespace Shop.Ordering;

public sealed class OrderNotFoundException(Guid orderId) : Exception($"Order {orderId} was not found.");
