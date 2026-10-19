namespace eShop.Catalog.Services;

internal static class BrandDataLoader
{
    [DataLoader]
    public static async Task<Dictionary<int, Brand>> GetBrandByIdAsync(
        IReadOnlyList<int> brandIds,
        [DataLoaderState("TenantId")] int tenantId,
        CatalogContext context,
        CancellationToken cancellationToken)
        => await context.Brands
            .Where(t => brandIds.Contains(t.Id))
            .MapToBrand()
            .Where(t => t.TenantId == tenantId)
            .ToDictionaryAsync(t => t.Id, cancellationToken);
}

public sealed class TenantIdAttribute()
    : DataLoaderStateAttribute("TenantId");