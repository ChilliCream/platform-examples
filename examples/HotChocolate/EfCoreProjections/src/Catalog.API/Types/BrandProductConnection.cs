using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

public class BrandProductConnection(int brandId, Page<Product> page) : PageConnection<Product>(page)
{
    public async Task<IReadOnlyList<ProductTypeAggregate>> GetAggregatesAsync(
        ProductTypeService productTypeService,
        CancellationToken cancellationToken) 
        => await productTypeService.GetAggregatesAsync(brandId, cancellationToken);
}