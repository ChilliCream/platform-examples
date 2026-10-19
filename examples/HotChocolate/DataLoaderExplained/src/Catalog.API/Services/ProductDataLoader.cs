namespace eShop.Catalog.Services;

internal static class ProductDataLoader
{

    [DataLoader]
    public static async Task<Dictionary<int, Product>> GetProductByIdAsync(
        IReadOnlyList<int> productIds,
        CatalogContext context,
        CancellationToken cancellationToken)
        => await context.Products
            .Where(t => productIds.Contains(t.Id))
            .MapToProduct()
            .ToDictionaryAsync(t => t.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<int, Product[]>> GetProductsByBrandIdAsync(
        IReadOnlyList<int> brandIds,
        CatalogContext context,
        CancellationToken cancellationToken)
        => await context.Products
            .Where(t => brandIds.Contains(t.BrandId))
            .MapToProduct()
            .GroupBy(t => t.BrandId)
            .Select(t => new { t.Key, Items = t.OrderBy(p => p.Name).ToArray() })
            .ToDictionaryAsync(t => t.Key, t => t.Items, cancellationToken);
}