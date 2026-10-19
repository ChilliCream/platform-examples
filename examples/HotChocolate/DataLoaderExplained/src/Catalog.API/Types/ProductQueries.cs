using eShop.Catalog.Services;

namespace eShop.Catalog.Types;

[QueryType]
public static partial class ProductQueries
{
    public static async Task<Product?> GetProductByIdAsync(
        int id,
        ProductService productService,
        CancellationToken cancellationToken)
        => await productService.GetProductByIdAsync(id, cancellationToken);

    public static async Task<Product[]> GetProductsAsync(
        ProductService productService,
        CancellationToken cancellationToken)
        => await productService.GetProductsAsync(cancellationToken);
}