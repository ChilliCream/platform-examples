namespace Shop.Billing;

public sealed class TransientException(string message) : Exception(message);
