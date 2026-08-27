using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using Bookshop.Domain.Authors;
using Bookshop.Domain.Books;
using Mocha.Mediator;

namespace Bookshop.Application.Books;

/// <summary>
/// Adds a new book to the catalog for an existing author. Fails when the ISBN is already
/// taken or the author does not exist.
/// </summary>
public sealed record AddBook(
    string Title,
    string Isbn,
    decimal Price,
    string Currency,
    Guid AuthorId,
    int InitialStock = 0) : ICommand<Guid>;

public sealed class AddBookHandler(
    IBookRepository books,
    IAuthorRepository authors,
    IUnitOfWork unitOfWork,
    IAuthorizationService authorizationService)
    : ICommandHandler<AddBook, Guid>
{
    public async ValueTask<Guid> HandleAsync(AddBook command, CancellationToken cancellationToken)
    {
        await authorizationService.EnsurePermissionAsync(Permissions.Books.Create, cancellationToken);

        if (await authors.GetAsync(command.AuthorId, cancellationToken) is null)
        {
            throw new AuthorNotFoundException(command.AuthorId);
        }

        var isbn = Book.NormalizeIsbn(command.Isbn);

        if (await books.GetByIsbnAsync(isbn, cancellationToken) is not null)
        {
            throw new DuplicateIsbnException(isbn);
        }

        var book = Book.Create(
            command.Title,
            isbn,
            command.Price,
            command.Currency,
            command.AuthorId,
            command.InitialStock);

        await books.AddAsync(book, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return book.Id;
    }
}
