using static eShop.Catalog.Services.Ordering;

namespace eShop.Catalog.Services;

public class ProductService
{
    private readonly CatalogContext _context;
    private readonly IProductByIdDataLoader _productById;
    private readonly IProductsByBrandIdDataLoader _productsByBrandId;

    internal ProductService(
        CatalogContext context,
        IProductByIdDataLoader productById,
        IProductsByBrandIdDataLoader productsByBrandId)
    {
        _context = context;
        _productById = productById;
        _productsByBrandId = productsByBrandId;
    }

    public async Task<Product?> GetProductByIdAsync(
        int id,
        QueryContext<Product>? query = default,
        CancellationToken cancellationToken = default)
        => await _productById
            .With(query)
            .LoadAsync(id, cancellationToken);

    public async Task<Page<Product>> GetProductsAsync(
        PagingArguments pagingArgs,
        QueryContext<Product>? query = default,
        CancellationToken cancellationToken = default)
        => await _context.Products
            .MapToProduct()
            .With(query, DefaultOrder)
            .ToPageAsync(pagingArgs, cancellationToken);

    public async Task<Page<Product>> GetProductsByBrandAsync(
        int brandId,
        PagingArguments pagingArgs,
        QueryContext<Product>? query = default,
        CancellationToken cancellationToken = default)
        => await _productsByBrandId
            .With(pagingArgs, query)
            .LoadAsync(brandId, cancellationToken)
            ?? Page<Product>.Empty;
}
