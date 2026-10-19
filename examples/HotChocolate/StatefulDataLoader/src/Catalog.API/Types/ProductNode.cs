using eShop.Catalog.Services;

namespace eShop.Catalog.Types;

[ObjectType<Product>]
public static partial class ProductNode
{
    public static async Task<Brand?> GetBrandAsync(
        [Parent(requires: nameof(Product.BrandId))] Product product,
        [GlobalState("TenantId")] int tenantId,
        BrandService brandService,
        CancellationToken cancellationToken)
        => await brandService.GetBrandByIdAsync(product.BrandId, tenantId, cancellationToken);
}