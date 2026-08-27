using Bookshop.Application.Authors.ReadModels;
using Bookshop.Application.Books;
using Bookshop.Application.Books.DataLoaders;
using Bookshop.Application.Books.ReadModels;
using Bookshop.GraphQL.Errors;
using GreenDonut.Data;
using HotChocolate.Types.Relay;
using Mocha.Mediator;
using AppSuccess = Bookshop.Application.Common.Success<System.Guid>;
using AppNotFound = Bookshop.Application.Common.NotFound;

namespace Bookshop.GraphQL.Books;

[MutationType]
internal static partial class BookMutations
{
    public static async Task<BookReadModel> AddBookAsync(
        string title,
        string isbn,
        decimal price,
        string currency,
        [ID<AuthorReadModel>] Guid authorId,
        int initialStock,
        IMediator mediator,
        QueryContext<BookReadModel> query,
        IBookByIdDataLoader bookById,
        CancellationToken cancellationToken)
    {
        var command = new AddBook(title, isbn, price, currency, authorId, initialStock);
        var bookId = await mediator.SendAsync(command, cancellationToken);

        return await bookById.With(query).LoadRequiredAsync(bookId, cancellationToken);
    }

    [Error(typeof(NotFound))]
    public static async Task<BookReadModel> RestockBookAsync(
        [ID<BookReadModel>] Guid bookId,
        int quantity,
        IMediator mediator,
        QueryContext<BookReadModel> query,
        IBookByIdDataLoader bookById,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new RestockBook(bookId, quantity), cancellationToken);

        var id = result switch
        {
            AppSuccess success => success.Value,
            AppNotFound notFound => throw new NotFound(notFound.Reason),
            null => throw new InvalidOperationException("RestockBook result was null."),
            _ => throw new InvalidOperationException("Unhandled RestockBook result."),
        };

        return await bookById.With(query).LoadRequiredAsync(id, cancellationToken);
    }

}
