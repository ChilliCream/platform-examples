using Bookshop.Application.Authors.DataLoaders;
using Bookshop.Application.Authors.ReadModels;
using Bookshop.Application.Books.DataLoaders;
using Bookshop.Application.Books.ReadModels;
using GreenDonut.Data;
using HotChocolate.Types.Relay;

namespace Bookshop.GraphQL.Books;

/// <summary>
/// GraphQL shape for <see cref="BookReadModel"/>.
/// </summary>
[ObjectType<BookReadModel>]
internal static partial class BookNode
{
    [ID<BookReadModel>]
    public static Guid GetId([Parent(requires: nameof(BookReadModel.Id))] BookReadModel book) => book.Id;

    static partial void Configure(IObjectTypeDescriptor<BookReadModel> descriptor)
        => descriptor.Name("Book");

    /// <summary>
    /// Author association, resolved through the by-id DataLoader. Every book has an author
    /// (<see cref="BookReadModel.AuthorId"/> is required), so this loads non-null.
    /// </summary>
    [BindMember(nameof(BookReadModel.AuthorId))]
    public static async Task<AuthorReadModel> GetAuthorAsync(
        [Parent(requires: nameof(BookReadModel.AuthorId))] BookReadModel book,
        QueryContext<AuthorReadModel> query,
        IAuthorByIdDataLoader authorById,
        CancellationToken cancellationToken)
        => await authorById.With(query).LoadRequiredAsync(book.AuthorId, cancellationToken);

    /// <summary>
    /// Relay node resolver for <see cref="BookReadModel"/>, backed by the same
    /// <c>IBookByIdDataLoader</c> the <c>bookById</c> root field uses.
    /// </summary>
    [NodeResolver]
    public static async Task<BookReadModel?> GetBookByIdAsync(
        Guid id,
        QueryContext<BookReadModel> query,
        IBookByIdDataLoader bookById,
        CancellationToken cancellationToken)
        => await bookById.With(query).LoadAsync(id, cancellationToken);
}
