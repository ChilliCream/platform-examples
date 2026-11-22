namespace Shop.Ordering;

/// <summary>
/// Indicates that the requested order could not be found.
/// </summary>
public sealed class OrderNotFoundException(Guid orderId) : Exception($"Order {orderId} was not found.");
