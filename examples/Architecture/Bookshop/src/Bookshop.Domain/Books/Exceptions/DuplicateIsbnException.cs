using Bookshop.Domain.Common;

namespace Bookshop.Domain.Books;

public sealed class DuplicateIsbnException(string isbn)
    : DomainException($"A book with ISBN '{isbn}' already exists.")
{
    public string Isbn { get; } = isbn;
}
