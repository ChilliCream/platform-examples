using Bookshop.Application.Books;
using Bookshop.Application.Books.DataLoaders;
using Bookshop.Application.Books.ReadModels;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;
using HotChocolate.Types.Relay;
using Mocha.Mediator;

namespace Bookshop.GraphQL.Books;

[QueryType]
internal static partial class BookQueries
{
    /// <summary>
    /// Paged browse dispatched to Application's DataLoader-backed query handler.
    /// </summary>
    [UseConnection]
    public static async Task<PageConnection<BookReadModel>> GetBooksAsync(
        PagingArguments pagingArguments,
        QueryContext<BookReadModel> query,
        IMediator mediator,
        CancellationToken cancellationToken)
        => await mediator.QueryAsync(new ListBooks(pagingArguments, query), cancellationToken);

    public static async Task<BookReadModel?> GetBookByIdAsync(
        [ID<BookReadModel>] Guid id,
        QueryContext<BookReadModel> query,
        IBookByIdDataLoader bookById,
        CancellationToken cancellationToken)
        => await bookById.With(query).LoadAsync(id, cancellationToken);
}
