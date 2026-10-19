using eShop.Catalog.Data.Entities;
using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

[QueryType]
public static partial class BrandQueries
{
    public static async Task<Brand?> GetBrandByIdAsync(
        int id,
        QueryContext<Brand> query,
        CatalogContext context,
        CancellationToken cancellationToken)
        => await context.Brands
            .MapToBrand()
            .With(query.Include(t => t.Id))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);


    public static async Task<Brand[]> GetBrandsAsync(
        QueryContext<Brand> query,
        CatalogContext context,
        CancellationToken cancellationToken)
        => await context.Brands
            .MapToBrand()
            .With(query.Include(t => t.Id))
            .ToArrayAsync(cancellationToken);
}
