namespace Bookshop.Domain.Books;

/// <summary>
/// Loads and stores <see cref="Book"/> aggregate roots as whole units.
/// </summary>
public interface IBookRepository
{
    Task<Book?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default);

    Task AddAsync(Book book, CancellationToken cancellationToken = default);
}
