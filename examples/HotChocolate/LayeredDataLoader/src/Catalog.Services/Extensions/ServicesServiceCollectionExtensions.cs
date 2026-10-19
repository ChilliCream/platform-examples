using eShop.Catalog.Data;
using eShop.Catalog.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServicesServiceCollectionExtensions
{
    public static IServiceCollection AddServiceLayer(
        this IServiceCollection services)
    {
        services.AddCatalogDataLoader();
        services.AddScoped(sp => new BrandService(
            sp.GetRequiredService<CatalogContext>(),
            sp.GetRequiredService<IBrandByIdDataLoader>()));
        services.AddScoped(sp => new ProductService(
            sp.GetRequiredService<CatalogContext>(),
            sp.GetRequiredService<IProductByIdDataLoader>(),
            sp.GetRequiredService<IProductsByBrandIdDataLoader>()));
        return services;
    }
}
