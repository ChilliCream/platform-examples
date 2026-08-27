using Bookshop.Application.Authors;
using Bookshop.Application.Authors.DataLoaders;
using Bookshop.Application.Authors.ReadModels;
using GreenDonut.Data;
using Mocha.Mediator;

namespace Bookshop.GraphQL.Authors;

[MutationType]
internal static partial class AuthorMutations
{
    /// <summary>
    /// AddAuthorHandler always succeeds and returns only the new author id (see
    /// Bookshop.Application/Authors/AddAuthor.cs) - no <c>Result&lt;T&gt;</c>, so no typed
    /// errors are declared for this mutation. The payload reloads the full
    /// <see cref="AuthorReadModel"/> through the by-id DataLoader.
    /// </summary>
    public static async Task<AuthorReadModel> AddAuthorAsync(
        string name,
        string? bio,
        IMediator mediator,
        QueryContext<AuthorReadModel> query,
        IAuthorByIdDataLoader authorById,
        CancellationToken cancellationToken)
    {
        var authorId = await mediator.SendAsync(new AddAuthor(name, bio), cancellationToken);

        return await authorById.With(query).LoadRequiredAsync(authorId, cancellationToken);
    }
}
