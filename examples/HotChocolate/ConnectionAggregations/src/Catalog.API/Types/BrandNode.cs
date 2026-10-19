using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

[ObjectType<Brand>]
public static partial class BrandNode
{
    [UseFiltering]
    [UseSorting]
    public static async Task<BrandProductsConnection> GetProductsAsync(
        [Parent(requires: nameof(Brand.Id))] Brand brand,
        PagingArguments pagingArgs,
        QueryContext<Product> query,
        ConnectionFlags connectionFlags,
        ProductService productService,
        CancellationToken cancellationToken)
    {
        Page<Product> page =
            connectionFlags is ConnectionFlags.None
                ? Page<Product>.Empty
                : await productService.GetProductsByBrandAsync(
                    brand.Id,
                    pagingArgs,
                    query,
                    cancellationToken);
        return new BrandProductsConnection(brand.Id, page);
    }
}

public class BrandProductsConnection(
    int brandId,
    Page<Product> page,
    int maxRelativeCursorCount = 5)
    : PageConnection<Product>(page, maxRelativeCursorCount)
{
    public Task<IReadOnlyList<ProductTypeAggregate>> GetAggregatesAsync(
        ProductTypeService productTypeService,
        CancellationToken cancellationToken)
        => productTypeService.GetAggregatesAsync(brandId, cancellationToken);
}


