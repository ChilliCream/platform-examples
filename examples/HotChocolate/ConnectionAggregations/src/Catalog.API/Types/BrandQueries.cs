using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

[QueryType]
public static partial class BrandQueries
{
    [NodeResolver]
    public static async Task<Brand?> GetBrandByIdAsync(
        int id,
        BrandService brandService,
        CancellationToken cancellationToken)
        => await brandService.GetBrandByIdAsync(id, cancellationToken);

    public static async Task<PageConnection<Brand>> GetBrandsAsync(
        PagingArguments pagingArgs,
        BrandService brandService,
        CancellationToken cancellationToken)
    {
        var page = await brandService.GetBrandsAsync(pagingArgs, cancellationToken);
        return new PageConnection<Brand>(page);
    }
}
