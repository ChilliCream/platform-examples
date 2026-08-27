using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bookshop.Infrastructure.Common.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddBookshopDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Bookshop")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Bookshop is not configured.");

        services.AddDbContext<BookshopContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddHostedService<BookshopMigrationHostedService>();

        return services;
    }
}
