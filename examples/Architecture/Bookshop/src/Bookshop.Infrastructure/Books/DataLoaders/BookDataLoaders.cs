using Bookshop.Application.Books.ReadModels;
using Bookshop.Application.Common;
using Bookshop.Infrastructure.Common.Database;
using GreenDonut;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Books.DataLoaders;

/// <summary>
/// Source-generated fetch methods backing <see cref="Bookshop.Application.Books.DataLoaders.IBookByIdDataLoader"/>
/// and <see cref="Bookshop.Application.Books.DataLoaders.IBooksByAuthorIdDataLoader"/>. The
/// generator emits matching <c>BookByIdDataLoader</c>/<c>BooksByAuthorIdDataLoader</c> classes
/// in this namespace; each is bound to its Application interface via a partial class
/// declaration in its own file next to this one (interface generation is turned off for this
/// assembly, see Properties/AssemblyInfo.cs).
/// </summary>
internal static class BookDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<Guid, BookReadModel>> GetBookByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<BookReadModel> query,
        BookshopContext context,
        CancellationToken cancellationToken)
        => await context.Books
            .AsNoTracking()
            .Where(book => ids.Contains(book.Id))
            .Select(ReadModelProjections.ToBookReadModel)
            .With(query.Include(book => book.Id))
            .ToDictionaryAsync(book => book.Id, cancellationToken);

    // The stable order ends in the key so cursor pagination remains deterministic.
    [DataLoader]
    public static async Task<Dictionary<Guid, Page<BookReadModel>>> GetBooksByAuthorIdAsync(
        IReadOnlyList<Guid> authorIds,
        PagingArguments pagingArguments,
        QueryContext<BookReadModel> query,
        BookshopContext context,
        CancellationToken cancellationToken)
        => await context.Books
            .AsNoTracking()
            .Where(book => authorIds.Contains(book.AuthorId))
            .Select(ReadModelProjections.ToBookReadModel)
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Id)
            .With(query.Include(book => book.AuthorId))
            .ToBatchPageAsync(book => book.AuthorId, pagingArguments, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<CatalogKey, Page<BookReadModel>>> GetBooksAsync(
        IReadOnlyList<CatalogKey> catalogs,
        PagingArguments pagingArguments,
        QueryContext<BookReadModel> query,
        BookshopContext context,
        CancellationToken cancellationToken)
    {
        if (!catalogs.Contains(CatalogKey.All))
        {
            return [];
        }

        var page = await context.Books
            .AsNoTracking()
            .Select(ReadModelProjections.ToBookReadModel)
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Id)
            .With(query.Include(book => book.Id))
            .ToPageAsync(pagingArguments, cancellationToken);

        return new Dictionary<CatalogKey, Page<BookReadModel>> { [CatalogKey.All] = page };
    }
}
