namespace Shop.Billing;

/// <summary>
/// Indicates a temporary failure during billing.
/// </summary>
public sealed class TransientException(string message) : Exception(message);
