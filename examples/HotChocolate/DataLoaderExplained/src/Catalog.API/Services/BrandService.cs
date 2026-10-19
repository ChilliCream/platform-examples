namespace eShop.Catalog.Services;

public class BrandService(
    CatalogContext context,
    IBrandByIdDataLoader brandById)
{
    public async Task<Brand?> GetBrandByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        => await brandById.LoadAsync(id, cancellationToken);

    public async Task<Brand[]> GetBrandsAsync(
        CancellationToken cancellationToken = default)
        => await context.Brands
            .MapToBrand()
            .OrderBy(b => b.Name)
            .ToArrayAsync(cancellationToken);
}
