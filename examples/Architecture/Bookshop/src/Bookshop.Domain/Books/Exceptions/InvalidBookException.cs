using Bookshop.Domain.Common;

namespace Bookshop.Domain.Books;

public sealed class InvalidBookException(string message) : DomainException(message);
