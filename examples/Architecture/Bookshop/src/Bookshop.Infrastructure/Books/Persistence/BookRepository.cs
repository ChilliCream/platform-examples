using Bookshop.Domain.Books;
using Bookshop.Infrastructure.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Books.Persistence;

/// <summary>
/// Loads and stores <see cref="Book"/> aggregate roots as tracked entities. Changes are
/// persisted only when <see cref="Bookshop.Application.Common.IUnitOfWork.SaveChangesAsync"/>
/// is called; this repository never calls <c>SaveChanges</c> itself.
/// </summary>
internal sealed class BookRepository(BookshopContext context) : IBookRepository
{
    public Task<Book?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Books.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
        => context.Books.SingleOrDefaultAsync(x => x.Isbn == isbn, cancellationToken);

    public Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        context.Books.Add(book);
        return Task.CompletedTask;
    }
}
