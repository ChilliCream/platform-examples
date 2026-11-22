namespace Shop.Ordering;

/// <summary>
/// Indicates that an order request failed validation.
/// </summary>
public sealed class OrderValidationException(string message) : Exception(message);
