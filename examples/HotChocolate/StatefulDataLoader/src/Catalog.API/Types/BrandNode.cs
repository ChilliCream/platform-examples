using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

[ObjectType<Brand>]
public static partial class BrandNode
{
    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Product>> GetProductsAsync(
        [Parent(requires: nameof(Brand.Id))] Brand brand,
        PagingArguments pagingArgs,
        QueryContext<Product> query,
        ProductService productService,
        CancellationToken cancellationToken)
    {
        var page = await productService.GetProductsByBrandAsync(
            brand.Id,
            pagingArgs,
            query,
            cancellationToken);
        return new PageConnection<Product>(page);
    }
}
