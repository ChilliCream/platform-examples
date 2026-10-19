namespace eShop.Catalog.Services;

public class ProductService(
    CatalogContext context,
    IProductByIdDataLoader productById,
    IProductsByBrandIdDataLoader productsByBrandId)
{
    public async Task<Product?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        => await productById.LoadAsync(id, cancellationToken);

    public async Task<Product[]> GetProductsAsync(
        CancellationToken cancellationToken = default)
        => await context.Products
            .MapToProduct()
            .OrderBy(b => b.Name)
            .ToArrayAsync(cancellationToken);

    public async Task<Product[]> GetProductsByBrandAsync(
        int brandId,
        CancellationToken cancellationToken = default)
        => await productsByBrandId.LoadAsync(brandId, cancellationToken) ?? [];
}
