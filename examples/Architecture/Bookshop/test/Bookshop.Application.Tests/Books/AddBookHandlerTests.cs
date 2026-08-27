using Bookshop.Application.Books;
using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using Bookshop.Application.Tests.Authors.Fakes;
using Bookshop.Application.Tests.Books.Fakes;
using Bookshop.Application.Tests.Common.Fakes;
using Bookshop.Domain.Authors;
using Bookshop.Domain.Books;

namespace Bookshop.Application.Tests.Books;

public sealed class AddBookHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_DenyBeforeValidationOrDataAccess_When_PermissionIsMissing()
    {
        var handler = new AddBookHandler(null!, null!, null!, new DenyAuthorizationService());
        var command = new AddBook("", "", -1m, "", Guid.Empty);

        var exception = await Assert.ThrowsAsync<AuthorizationDeniedException>(
            async () => await handler.HandleAsync(command, CancellationToken.None));

        Assert.Equal(Permissions.Books.Create, exception.Permission);
    }

    [Fact]
    public async Task HandleAsync_Should_ThrowDuplicateIsbnException_When_IsbnAlreadyExists()
    {
        // arrange
        var authors = new InMemoryAuthorRepository();
        var author = Author.Create("Robert C. Martin");
        await authors.AddAsync(author, CancellationToken.None);

        var books = new InMemoryBookRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new AddBookHandler(books, authors, unitOfWork, new AllowAllAuthorizationService());

        const string isbn = "9780132350884";
        var firstResult = await handler.HandleAsync(
            new AddBook("Clean Code", isbn, 39.99m, "USD", author.Id),
            CancellationToken.None);
        Assert.NotEqual(Guid.Empty, firstResult);

        // act
        var exception = await Assert.ThrowsAsync<DuplicateIsbnException>(
            async () => await handler.HandleAsync(
                new AddBook("Clean Code (2nd copy)", isbn, 42.00m, "USD", author.Id),
                CancellationToken.None));

        // assert
        Assert.Equal(isbn, exception.Isbn);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_Should_ThrowInvalidBookException_When_IsbnIsMalformed()
    {
        // arrange
        var authors = new InMemoryAuthorRepository();
        var author = Author.Create("Robert C. Martin");
        await authors.AddAsync(author, CancellationToken.None);

        var books = new InMemoryBookRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new AddBookHandler(books, authors, unitOfWork, new AllowAllAuthorizationService());

        // act
        await Assert.ThrowsAsync<InvalidBookException>(
            async () => await handler.HandleAsync(
                new AddBook("Clean Code", "not-an-isbn", 39.99m, "USD", author.Id),
                CancellationToken.None));

        // assert
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_Should_ThrowAuthorNotFoundException_When_AuthorDoesNotExist()
    {
        // arrange
        var authors = new InMemoryAuthorRepository();
        var books = new InMemoryBookRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new AddBookHandler(books, authors, unitOfWork, new AllowAllAuthorizationService());

        // act
        await Assert.ThrowsAsync<AuthorNotFoundException>(
            async () => await handler.HandleAsync(
                new AddBook("Clean Code", "9780132350884", 39.99m, "USD", Guid.NewGuid()),
                CancellationToken.None));

        // assert
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_Should_AddBook_When_IsbnAndAuthorAreValid()
    {
        // arrange
        var authors = new InMemoryAuthorRepository();
        var author = Author.Create("Robert C. Martin");
        await authors.AddAsync(author, CancellationToken.None);

        var books = new InMemoryBookRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new AddBookHandler(books, authors, unitOfWork, new AllowAllAuthorizationService());

        // act
        var result = await handler.HandleAsync(
            new AddBook("Clean Code", "9780132350884", 39.99m, "USD", author.Id, InitialStock: 5),
            CancellationToken.None);

        // assert
        var stored = await books.GetAsync(result, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Equal(5, stored.StockQuantity);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private sealed class DenyAuthorizationService : IAuthorizationService
    {
        public ValueTask<bool> HasPermissionAsync(
            string permission,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(false);
    }
}
