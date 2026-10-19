using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Execution.Processing;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

[QueryType]
public static partial class ProductQueries
{
    [NodeResolver]
    public static async Task<Product?> GetProductByIdAsync(
        int id,
        IProductByIdDataLoader productById,
        CancellationToken cancellationToken)
        => await productById.LoadAsync(id, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Product>> GetProductsAsync(
        PagingArguments pagingArgs,
        QueryContext<Product> query,
        CatalogContext context,
        CancellationToken cancellationToken)
    {
        var page = await context.Products
            .MapToReadModel()
            .With(query)
            .OrderBy(t => t.Id)
            .ToPageAsync(pagingArgs, cancellationToken);
        return new PageConnection<Product>(page);
    }
}