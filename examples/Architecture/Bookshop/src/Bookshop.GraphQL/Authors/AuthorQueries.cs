using Bookshop.Application.Authors;
using Bookshop.Application.Authors.DataLoaders;
using Bookshop.Application.Authors.ReadModels;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;
using HotChocolate.Types.Relay;
using Mocha.Mediator;

namespace Bookshop.GraphQL.Authors;

[QueryType]
internal static partial class AuthorQueries
{
    /// <summary>
    /// Paged browse dispatched to Application's DataLoader-backed query handler.
    /// </summary>
    [UseConnection]
    public static async Task<PageConnection<AuthorReadModel>> GetAuthorsAsync(
        PagingArguments pagingArguments,
        QueryContext<AuthorReadModel> query,
        IMediator mediator,
        CancellationToken cancellationToken)
        => await mediator.QueryAsync(new ListAuthors(pagingArguments, query), cancellationToken);

    public static async Task<AuthorReadModel?> GetAuthorByIdAsync(
        [ID<AuthorReadModel>] Guid id,
        QueryContext<AuthorReadModel> query,
        IAuthorByIdDataLoader authorById,
        CancellationToken cancellationToken)
        => await authorById.With(query).LoadAsync(id, cancellationToken);
}
