using GreenDonut.Data;

namespace eShop.Catalog.Services;

public class ProductTypeService(
    CatalogContext context,
    IProductTypeByIdDataLoader productTypeById,
    IProductTypeAggregatesByBrandIdDataLoader productTypeAggregatesByBrandId)
{
    public async Task<ProductType?> GetProductTypeByIdAsync(
        int id,
        CancellationToken cancellationToken)
        => await productTypeById.LoadAsync(id, cancellationToken);

    public async Task<Page<ProductType>> GetProductTypesAsync(
        PagingArguments pagingArgs,
        CancellationToken cancellationToken)
        => await context.ProductTypes
            .MapToProductType()
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Id)
            .ToPageAsync(pagingArgs, cancellationToken);

    public async Task<IReadOnlyList<ProductTypeAggregate>> GetAggregatesAsync(
        int brandId,
        CancellationToken cancellationToken)
        => await productTypeAggregatesByBrandId.LoadAsync(brandId, cancellationToken) ?? [];
}