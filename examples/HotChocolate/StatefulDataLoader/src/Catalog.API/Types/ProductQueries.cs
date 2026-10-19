using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

[QueryType]
public static partial class ProductQueries
{
    public static async Task<Product?> GetProductByIdAsync(
        int id,
        QueryContext<Product> query,
        ProductService productService,
        CancellationToken cancellationToken)
        => await productService.GetProductByIdAsync(id, 1, query, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Product>> GetProductsAsync(
        PagingArguments pagingArgs,
        QueryContext<Product> query,
        ProductService productService,
        CancellationToken cancellationToken)
    {
        var page = await productService.GetProductsAsync(
            pagingArgs,
            query,
            cancellationToken);
        return new PageConnection<Product>(page);
    }
}