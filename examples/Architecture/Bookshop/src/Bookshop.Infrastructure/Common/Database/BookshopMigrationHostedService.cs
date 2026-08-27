using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bookshop.Infrastructure.Common.Database;

/// <summary>
/// Applies pending EF Core migrations to the Bookshop database on host startup. Unlike the
/// ChilliCream init-container machinery this runs in-process as a regular hosted service.
/// </summary>
internal sealed class BookshopMigrationHostedService(IServiceScopeFactory scopeFactory)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookshopContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
