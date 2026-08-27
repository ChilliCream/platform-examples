using Bookshop.Domain.Authors;

namespace Bookshop.Application.Tests.Authors.Fakes;

public sealed class InMemoryAuthorRepository : IAuthorRepository
{
    private readonly Dictionary<Guid, Author> _authors = [];

    public Task<Author?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_authors.GetValueOrDefault(id));

    public Task AddAsync(Author author, CancellationToken cancellationToken = default)
    {
        _authors[author.Id] = author;
        return Task.CompletedTask;
    }
}
