using Bookshop.Application.Authors.DataLoaders;
using Bookshop.Application.Authors.ReadModels;
using Bookshop.Application.Books.DataLoaders;
using Bookshop.Application.Books.ReadModels;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;
using HotChocolate.Types.Relay;

namespace Bookshop.GraphQL.Authors;

/// <summary>
/// GraphQL shape for <see cref="AuthorReadModel"/>.
/// </summary>
[ObjectType<AuthorReadModel>]
internal static partial class AuthorNode
{
    [ID<AuthorReadModel>]
    public static Guid GetId([Parent(requires: nameof(AuthorReadModel.Id))] AuthorReadModel author)
        => author.Id;

    static partial void Configure(IObjectTypeDescriptor<AuthorReadModel> descriptor)
        => descriptor.Name("Author");

    /// <summary>
    /// Relay node resolver for <see cref="AuthorReadModel"/>, backed by the same
    /// <c>IAuthorByIdDataLoader</c> the <c>authorById</c> root field uses.
    /// </summary>
    [NodeResolver]
    public static async Task<AuthorReadModel?> GetAuthorByIdAsync(
        Guid id,
        QueryContext<AuthorReadModel> query,
        IAuthorByIdDataLoader authorById,
        CancellationToken cancellationToken)
        => await authorById.With(query).LoadAsync(id, cancellationToken);

    /// <summary>
    /// Paged association field for the books written by this author. Uses the batch-paging
    /// <c>IBooksByAuthorIdDataLoader</c> (inline <c>OrderBy(Title).ThenBy(Id)</c> per
    /// pagination.md, since this field has no client-controlled sorting per the R3b ruling: no
    /// <c>UseSorting</c> on this field unless paired with a <c>SortDefinition</c> default-order
    /// change in Bookshop.Application).
    /// </summary>
    [UseConnection]
    public static async Task<PageConnection<BookReadModel>> GetBooksAsync(
        [Parent(requires: nameof(AuthorReadModel.Id))] AuthorReadModel author,
        PagingArguments pagingArguments,
        QueryContext<BookReadModel> query,
        IBooksByAuthorIdDataLoader booksByAuthorId,
        CancellationToken cancellationToken)
        => await booksByAuthorId.With(pagingArguments, query).LoadAsync(author.Id, cancellationToken)
            ?? Page<BookReadModel>.Empty;
}
