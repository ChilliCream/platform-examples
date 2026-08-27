namespace Bookshop.GraphQL.Errors;

/// <summary>
/// A business rule was violated in a way not covered by a more specific typed error
/// (<see cref="NotFound"/> and <see cref="InsufficientStock"/>).
/// </summary>
public sealed class ValidationFailed(string message) : Exception(message)
{
}
