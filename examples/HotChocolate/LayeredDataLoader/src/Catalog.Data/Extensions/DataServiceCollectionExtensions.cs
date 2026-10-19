using eShop.Catalog.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddDataLayer(
        this IServiceCollection services,
        string? connectionString)
    {
        return services
            .AddDbContextPool<CatalogContext>(
                o => o.UseNpgsql(connectionString));
    }
}
