namespace eShop.Catalog.Services;

public class BrandService
{
    private readonly CatalogContext _context;
    private readonly IBrandByIdDataLoader _brandById;

    internal BrandService(
        CatalogContext context,
        IBrandByIdDataLoader brandById)
    {
        _context = context;
        _brandById = brandById;
    }

    public async Task<Brand?> GetBrandByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        => await _brandById.LoadAsync(id, cancellationToken);

    public async Task<Page<Brand>> GetBrandsAsync(
        PagingArguments pagingArgs,
        CancellationToken cancellationToken = default)
        => await _context.Brands
            .MapToBrand()
            .OrderBy(b => b.Name)
            .ThenBy(t => t.Id)
            .ToPageAsync(pagingArgs, cancellationToken);
}
