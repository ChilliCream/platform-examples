namespace Bookshop.GraphQL.Errors;

/// <summary>
/// A referenced entity (book, author, order) does not exist. Maps
/// <c>Bookshop.Application.Common.NotFound</c> onto a mutation-convention
/// typed error, reused across every mutation that can fail this way.
/// </summary>
public sealed class NotFound(string message) : Exception(message)
{
}
