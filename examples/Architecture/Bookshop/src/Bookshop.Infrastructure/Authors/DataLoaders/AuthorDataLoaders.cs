using Bookshop.Application.Authors.ReadModels;
using Bookshop.Application.Common;
using Bookshop.Infrastructure.Common.Database;
using GreenDonut;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Authors.DataLoaders;

/// <summary>
/// Source-generated fetch method backing <see cref="Bookshop.Application.Authors.DataLoaders.IAuthorByIdDataLoader"/>.
/// The generator emits a matching <c>AuthorByIdDataLoader</c> class in this namespace; it is
/// bound to the Application interface via a partial class declaration in
/// <c>AuthorByIdDataLoader.cs</c> next to this file (interface generation is turned off for
/// this assembly, see Properties/AssemblyInfo.cs).
/// </summary>
internal static class AuthorDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<Guid, AuthorReadModel>> GetAuthorByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<AuthorReadModel> query,
        BookshopContext context,
        CancellationToken cancellationToken)
        => await context
            .Authors.AsNoTracking()
            .Where(author => ids.Contains(author.Id))
            .Select(ReadModelProjections.ToAuthorReadModel)
            .With(query.Include(author => author.Id))
            .ToDictionaryAsync(author => author.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<CatalogKey, Page<AuthorReadModel>>> GetAuthorsAsync(
        IReadOnlyList<CatalogKey> catalogs,
        PagingArguments pagingArguments,
        QueryContext<AuthorReadModel> query,
        BookshopContext context,
        CancellationToken cancellationToken)
    {
        if (!catalogs.Contains(CatalogKey.All))
        {
            return [];
        }

        var page = await context.Authors
            .AsNoTracking()
            .Select(ReadModelProjections.ToAuthorReadModel)
            .OrderBy(author => author.Name)
            .ThenBy(author => author.Id)
            .With(query.Include(author => author.Id))
            .ToPageAsync(pagingArguments, cancellationToken);

        return new Dictionary<CatalogKey, Page<AuthorReadModel>> { [CatalogKey.All] = page };
    }
}
