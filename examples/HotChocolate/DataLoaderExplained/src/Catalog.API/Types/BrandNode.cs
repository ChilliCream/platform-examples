using eShop.Catalog.Services;

namespace eShop.Catalog.Types;

[ObjectType<Brand>]
public static partial class BrandNode
{
    public static async Task<Product[]> GetProductsAsync(
        [Parent] Brand brand,
        ProductService productService,
        CancellationToken cancellationToken)
        => await productService.GetProductsByBrandAsync(brand.Id, cancellationToken);
}
