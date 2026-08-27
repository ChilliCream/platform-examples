namespace Bookshop.Domain.Common;

/// <summary>
/// Base type for expected failures caused by domain rules.
/// </summary>
public abstract class DomainException(string message) : Exception(message);
