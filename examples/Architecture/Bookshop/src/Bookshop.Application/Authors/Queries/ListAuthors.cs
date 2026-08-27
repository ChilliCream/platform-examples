using Bookshop.Application.Authors.DataLoaders;
using Bookshop.Application.Authors.ReadModels;
using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using GreenDonut.Data;
using Mocha.Mediator;

namespace Bookshop.Application.Authors;

/// <summary>
/// Pages the full author catalog. The query and handler stay in Application while all database
/// access is delegated to the catalog DataLoader implemented by Infrastructure.
/// </summary>
public sealed record ListAuthors(PagingArguments Paging, QueryContext<AuthorReadModel> Query)
    : IQuery<Page<AuthorReadModel>>;

public sealed class ListAuthorsHandler(
    IAuthorsDataLoader authors,
    IAuthorizationService authorizationService)
    : IQueryHandler<ListAuthors, Page<AuthorReadModel>>
{
    public async ValueTask<Page<AuthorReadModel>> HandleAsync(
        ListAuthors query,
        CancellationToken cancellationToken)
    {
        await authorizationService.EnsurePermissionAsync(Permissions.Authors.Read, cancellationToken);

        return await authors
                .With(query.Paging, query.Query)
                .LoadAsync(CatalogKey.All, cancellationToken)
            ?? Page<AuthorReadModel>.Empty;
    }
}
