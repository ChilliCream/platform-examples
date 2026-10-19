using GreenDonut.Data;
using static eShop.Catalog.Services.Ordering;

namespace eShop.Catalog.Services;

internal static class ProductDataLoader
{

    [DataLoader]
    public static async Task<Dictionary<int, Product>> GetProductByIdAsync(
        IReadOnlyList<int> productIds,
        QueryContext<Product> query,
        CatalogContext context,
        CancellationToken cancellationToken)
        => await context.Products
            .Where(t => productIds.Contains(t.Id))
            .MapToReadModel()
            .With(query)
            .ToDictionaryAsync(t => t.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<int, Page<Product>>> GetProductsByBrandIdAsync(
        IReadOnlyList<int> brandIds,
        PagingArguments pagingArgs,
        QueryContext<Product> query,
        CatalogContext context,
        CancellationToken cancellationToken)
        => await context.Products
            .Where(t => brandIds.Contains(t.BrandId))
            .MapToReadModel()
            .With(query, DefaultOrder)
            .ToBatchPageAsync(t => t.BrandId, pagingArgs, cancellationToken);
    
    [DataLoader]
    public static async Task<Dictionary<int, Page<Product>>> GetProductsByTypeIdAsync(
        IReadOnlyList<int> typeIds,
        PagingArguments pagingArgs,
        QueryContext<Product> query,
        CatalogContext context,
        CancellationToken cancellationToken)
        => await context.Products
            .Where(t => typeIds.Contains(t.TypeId))
            .MapToReadModel()
            .With(query, DefaultOrder)
            .ToBatchPageAsync(t => t.TypeId, pagingArgs, cancellationToken);
}