using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

[ObjectType<ProductType>]
public static partial class ProductTypeNode
{
    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Product>> GetProductsAsync(
        [Parent(requires: nameof(ProductType.Id))] ProductType type,
        PagingArguments pagingArgs,
        QueryContext<Product> query,
        ProductService productService,
        CancellationToken cancellationToken)
    {
        var page = await productService.GetProductsByTypeAsync(
            type.Id,
            pagingArgs,
            query,
            cancellationToken);
        return new PageConnection<Product>(page);
    }
}