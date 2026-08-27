using Bookshop.Application.Books.DataLoaders;
using Bookshop.Application.Books.ReadModels;
using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using GreenDonut.Data;
using Mocha.Mediator;

namespace Bookshop.Application.Books;

/// <summary>
/// Pages the full book catalog. The query and handler stay in Application while all database
/// access is delegated to the catalog DataLoader implemented by Infrastructure.
/// </summary>
public sealed record ListBooks(PagingArguments Paging, QueryContext<BookReadModel> Query)
    : IQuery<Page<BookReadModel>>;

public sealed class ListBooksHandler(
    IBooksDataLoader books,
    IAuthorizationService authorizationService)
    : IQueryHandler<ListBooks, Page<BookReadModel>>
{
    public async ValueTask<Page<BookReadModel>> HandleAsync(
        ListBooks query,
        CancellationToken cancellationToken)
    {
        await authorizationService.EnsurePermissionAsync(Permissions.Books.Read, cancellationToken);

        return await books
                .With(query.Paging, query.Query)
                .LoadAsync(CatalogKey.All, cancellationToken)
            ?? Page<BookReadModel>.Empty;
    }
}
