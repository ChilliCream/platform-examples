using Bookshop.Domain.Common;

namespace Bookshop.Domain.Authors;

public sealed class AuthorNotFoundException(Guid authorId)
    : DomainException($"Author '{authorId}' was not found.")
{
    public Guid AuthorId { get; } = authorId;
}
