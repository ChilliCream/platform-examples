using eShop.Catalog.Data.Entities;
using eShop.Catalog.Services;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.StaticAssets;

namespace eShop.Catalog.Types;

[ObjectType<Brand>]
public static partial class BrandNode
{
    public static async Task<PageConnection<Product>> GetProductsAsync(
        [Parent(requires: nameof(Brand.Id))] Brand brand,
        PagingArguments pagingArguments,
        QueryContext<Product> query,
        IProductsByBrandIdDataLoader productsByBrand,
        CancellationToken cancellationToken)
    {
        var page = await productsByBrand
            .With(pagingArguments, query)
            .LoadAsync(brand.Id, cancellationToken)
                ?? Page<Product>.Empty;

        return new PageConnection<Product>(page);
    }
}