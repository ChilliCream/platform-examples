using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using Bookshop.Domain.Books;
using Mocha.Mediator;

namespace Bookshop.Application.Books;

/// <summary>
/// Increases the stock quantity of an existing book. Fails when the book does not exist.
/// </summary>
public sealed record RestockBook(Guid BookId, int Quantity) : ICommand<Result<Guid>>;

public sealed class RestockBookHandler(
    IBookRepository books,
    IUnitOfWork unitOfWork,
    IAuthorizationService authorizationService)
    : ICommandHandler<RestockBook, Result<Guid>>
{
    public async ValueTask<Result<Guid>> HandleAsync(RestockBook command, CancellationToken cancellationToken)
    {
        await authorizationService.EnsurePermissionAsync(Permissions.Books.Restock, cancellationToken);

        var book = await books.GetAsync(command.BookId, cancellationToken);

        if (book is null)
        {
            return new NotFound($"Book '{command.BookId}' was not found.");
        }

        book.Restock(command.Quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new Success<Guid>(book.Id);
    }
}
