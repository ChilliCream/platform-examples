using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bookshop.Infrastructure.Common.Database;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> commands can create a <see cref="BookshopContext"/>
/// without running the host.
/// </summary>
public sealed class BookshopContextFactory : IDesignTimeDbContextFactory<BookshopContext>
{
    public BookshopContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BookshopContext>()
            .UseNpgsql(
                Environment.GetEnvironmentVariable("BOOKSHOP_MIGRATION_CONNECTION")
                    ?? "Host=localhost;Database=bookshop;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new BookshopContext(options);
    }
}
