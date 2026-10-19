using eShop.Catalog.Services;

namespace eShop.Catalog.Types;

[QueryType]
public static partial class BrandQueries
{
    public static async Task<Brand?> GetBrandByIdAsync(
        int id,
        BrandService brandService,
        CancellationToken cancellationToken)
        => await brandService.GetBrandByIdAsync(id, cancellationToken);

    public static async Task<Brand[]> GetBrandsAsync(
        BrandService brandService,
        CancellationToken cancellationToken)
        => await brandService.GetBrandsAsync(cancellationToken);
}