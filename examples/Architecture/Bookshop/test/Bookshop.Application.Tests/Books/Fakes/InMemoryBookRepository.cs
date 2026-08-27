using Bookshop.Domain.Books;

namespace Bookshop.Application.Tests.Books.Fakes;

public sealed class InMemoryBookRepository : IBookRepository
{
    private readonly Dictionary<Guid, Book> _books = [];

    public Task<Book?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_books.GetValueOrDefault(id));

    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
        => Task.FromResult(_books.Values.SingleOrDefault(book => book.Isbn == isbn));

    public Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        _books[book.Id] = book;
        return Task.CompletedTask;
    }
}
