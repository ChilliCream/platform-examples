namespace eShop.Catalog.Services;

public static class ProductTypeDataLoader 
{
    [DataLoader]
    public static async Task<Dictionary<int, ProductType>> GetProductTypeByIdAsync(
        IReadOnlyList<int> typeIds,
        CatalogContext context,
        CancellationToken cancellationToken)
        => await context.ProductTypes
            .Where(t => typeIds.Contains(t.Id))
            .MapToProductType()
            .ToDictionaryAsync(t => t.Id, cancellationToken);
    
    [DataLoader]
    public static async Task<Dictionary<int, List<ProductTypeAggregate>>> GetProductTypeAggregatesByBrandIdAsync(
        IReadOnlyList<int> brandIds,
        CatalogContext context,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(string.Join(", ", brandIds));
        
        return await context.Products
            .Where(t => brandIds.Contains(t.BrandId))
            .GroupBy(t => new { t.BrandId, t.Type!.Name })
            .Select(g => new
            {
                g.Key.BrandId,
                Type = g.Key.Name,
                AveragePrice = g.Average(t => t.Price),
                MaxPrice = g.Max(t => t.Price),
                MinPrice = g.Min(t => t.Price),
                Count = g.Count()
            })
            .GroupBy(a => a.BrandId)
            .Select(g => new
            {
                BrandId = g.Key,
                ProductTypeAggregates = 
                    g.Select(a => new ProductTypeAggregate
                    {
                        Type = a.Type,
                        AveragePrice = a.AveragePrice,
                        MaxPrice = a.MaxPrice,
                        MinPrice = a.MinPrice,
                        Count = a.Count
                    }).ToList()
            })
            .ToDictionaryAsync(t => t.BrandId, t => t.ProductTypeAggregates, cancellationToken);
    }
}