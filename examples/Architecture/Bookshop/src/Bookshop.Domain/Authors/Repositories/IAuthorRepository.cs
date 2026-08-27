namespace Bookshop.Domain.Authors;

/// <summary>
/// Loads and stores <see cref="Author"/> aggregate roots as whole units.
/// </summary>
public interface IAuthorRepository
{
    Task<Author?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Author author, CancellationToken cancellationToken = default);
}
