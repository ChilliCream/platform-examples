using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

[QueryType]
public static partial class ProductTypeQueries
{
    [NodeResolver]
    public static Task<ProductType?> GetProductTypeByIdAsync(
        int id,
        ProductTypeService productTypeService,
        CancellationToken cancellationToken)
        => productTypeService.GetProductTypeByIdAsync(id, cancellationToken);
    
    public static async Task<PageConnection<ProductType>> GetProductTypesAsync(
        PagingArguments pagingArgs,
        ProductTypeService productTypeService,
        CancellationToken cancellationToken)
    {
        var page = await productTypeService.GetProductTypesAsync(pagingArgs, cancellationToken);
        return new PageConnection<ProductType>(page);
    }
}