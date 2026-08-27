using Bookshop.Domain.Authors;
using Bookshop.Infrastructure.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Authors.Persistence;

/// <summary>
/// Loads and stores <see cref="Author"/> aggregate roots as tracked entities. Changes are
/// persisted only when <see cref="Bookshop.Application.Common.IUnitOfWork.SaveChangesAsync"/>
/// is called; this repository never calls <c>SaveChanges</c> itself.
/// </summary>
internal sealed class AuthorRepository(BookshopContext context) : IAuthorRepository
{
    public Task<Author?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Authors.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Author author, CancellationToken cancellationToken = default)
    {
        context.Authors.Add(author);
        return Task.CompletedTask;
    }
}
