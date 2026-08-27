using Bookshop.Application.Common;
using Bookshop.Domain.Authors;
using Bookshop.Domain.Books;
using Bookshop.Domain.Customers;
using Bookshop.Infrastructure.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bookshop.Host.Host;

/// <summary>
/// Seeds a handful of authors, books, and one customer for local development. Registered only
/// in Development (see <c>Program.cs</c>) and skipped entirely when any book already exists, so
/// it is safe to run on every startup. Registered after
/// <see cref="BookshopMigrationHostedService"/> so it always runs against an up-to-date schema
/// (<see cref="IHostedService"/> instances start in registration order).
/// </summary>
/// <remarks>
/// Writes through the Domain-defined aggregate repositories (<see cref="IAuthorRepository"/>,
/// <see cref="IBookRepository"/>, <see cref="ICustomerRepository"/>) plus
/// <see cref="IUnitOfWork"/>, the same write path the Application command handlers use; there
/// is no bulk-insert shortcut, since the repositories only expose whole-aggregate operations.
/// The existence check queries <see cref="BookshopContext"/> directly (cheaper than loading
/// aggregates just to count them) rather than adding a query-only method to the write-side
/// repository interface.
/// </remarks>
internal sealed class BookshopSeedHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookshopContext>();

        if (await context.Books.AnyAsync(cancellationToken))
        {
            return;
        }

        var authors = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        var books = scope.ServiceProvider.GetRequiredService<IBookRepository>();
        var customers = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var leGuin = Author.Create(
            "Ursula K. Le Guin", "American author known for speculative fiction.");
        var asimov = Author.Create(
            "Isaac Asimov", "American writer and professor of biochemistry.");
        var christie = Author.Create(
            "Agatha Christie", "English writer known for detective novels.");

        await authors.AddAsync(leGuin, cancellationToken);
        await authors.AddAsync(asimov, cancellationToken);
        await authors.AddAsync(christie, cancellationToken);

        await books.AddAsync(Book.Create(
            "The Left Hand of Darkness", "9780306406157",
            15.99m, "USD", leGuin.Id, initialStock: 12), cancellationToken);
        await books.AddAsync(Book.Create(
            "The Dispossessed", "9780140283334",
            14.50m, "USD", leGuin.Id, initialStock: 8), cancellationToken);
        await books.AddAsync(Book.Create(
            "Foundation", "9780451234568",
            12.99m, "USD", asimov.Id, initialStock: 20), cancellationToken);
        await books.AddAsync(Book.Create(
            "I, Robot", "9780061120084",
            11.49m, "USD", asimov.Id, initialStock: 15), cancellationToken);
        await books.AddAsync(Book.Create(
            "Murder on the Orient Express", "9781594201202",
            13.25m, "USD", christie.Id, initialStock: 10), cancellationToken);
        await books.AddAsync(Book.Create(
            "And Then There Were None", "9780439358064",
            10.99m, "USD", christie.Id, initialStock: 5), cancellationToken);

        await customers.AddAsync(
            Customer.Create("Jane Reader", "jane.reader@example.com"), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
