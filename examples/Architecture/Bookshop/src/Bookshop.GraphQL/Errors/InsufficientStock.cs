namespace Bookshop.GraphQL.Errors;

/// <summary>
/// Placing an order failed because a requested book does not have enough stock.
/// </summary>
public sealed class InsufficientStock(string message) : Exception(message)
{
}
