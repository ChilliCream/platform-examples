using eShop.Catalog.Data.Entities;
using eShop.Catalog.Services.Errors;
using GreenDonut.Data;
using static eShop.Catalog.Services.Ordering;

namespace eShop.Catalog.Services;

public class ProductService(
    CatalogContext context,
    IProductByIdDataLoader productById,
    IProductsByBrandIdDataLoader productsByBrandId,
    IProductsByTypeIdDataLoader productsByTypeId)
{
    public async Task<Product?> GetProductByIdAsync(
        int id,
        QueryContext<Product>? query = default,
        CancellationToken cancellationToken = default)
        => await productById
            .With(query)
            .LoadAsync(id, cancellationToken);

    public async Task<Page<Product>> GetProductsAsync(
        PagingArguments pagingArgs,
        QueryContext<Product>? query = default,
        CancellationToken cancellationToken = default)
        => await context.Products
            .MapToReadModel()
            .With(query, DefaultOrder)
            .ToPageAsync(pagingArgs, cancellationToken);

    public async Task<Page<Product>> GetProductsByBrandAsync(
        int brandId,
        PagingArguments pagingArgs,
        QueryContext<Product>? query = default,
        CancellationToken cancellationToken = default)
        => await productsByBrandId
            .With(pagingArgs, query)
            .LoadAsync(brandId, cancellationToken)
            ?? Page<Product>.Empty;
    
    public async Task<Page<Product>> GetProductsByTypeAsync(
        int typeId,
        PagingArguments pagingArgs,
        QueryContext<Product>? query = default,
        CancellationToken cancellationToken = default)
        => await productsByTypeId
            .With(pagingArgs, query)
            .LoadAsync(typeId, cancellationToken)
            ?? Page<Product>.Empty;

    public async Task CreateProductAsync(Product product, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(product.Name);

        if (product.RestockThreshold >= product.MaxStockThreshold)
        {
            throw new MaxStockThresholdToSmallException(product.RestockThreshold, product.MaxStockThreshold);
        }

        if (!await context.Brands.AnyAsync(t => t.Id == product.BrandId, cancellationToken))
        {
            throw new BrandNotFoundException(product.BrandId);
        }

        if (!await context.ProductTypes.AnyAsync(t => t.Id == product.TypeId, cancellationToken))
        {
            throw new ProductTypeNotFoundException(product.TypeId);
        }

        var entity = new ProductEntity();
        product.MapToEntity(entity);
        context.Products.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken)
    {
        if (product.Id < 1)
        {
            throw new InvalidOperationException("Invalid product id.");
        }

        ArgumentException.ThrowIfNullOrEmpty(product.Name);

        if (product.RestockThreshold >= product.MaxStockThreshold)
        {
            throw new MaxStockThresholdToSmallException(product.RestockThreshold, product.MaxStockThreshold);
        }

        if (!await context.Brands.AnyAsync(t => t.Id == product.BrandId, cancellationToken))
        {
            throw new BrandNotFoundException(product.BrandId);
        }

        if (!await context.ProductTypes.AnyAsync(t => t.Id == product.TypeId, cancellationToken))
        {
            throw new ProductTypeNotFoundException(product.TypeId);
        }

        var entity = new ProductEntity();
        product.MapToEntity(entity);
        context.Products.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RenameProductAsync(int id, string newName, CancellationToken cancellationToken)
    {
        if (id < 1)
        {
            throw new InvalidOperationException("Invalid product id.");
        }

        ArgumentException.ThrowIfNullOrEmpty(newName);

        var affectedRows = await context.Products
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(t => t.SetProperty(p => p.Name, newName), cancellationToken);

        if (affectedRows == 0)
        {
            throw new ProductNotFoundException(id);
        }
    }
    
    public async Task ChangeProductPriceAsync(
        int id, 
        decimal newPrice, 
        CancellationToken cancellationToken)
    {
        if (id < 1)
        {
            throw new InvalidOperationException("Invalid product id.");
        }

        if (newPrice < 0)
        {
            throw new ProductPriceCannotBeNegative(newPrice);
        }

        var affectedRows = await context.Products
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(t => t.SetProperty(p => p.Price, newPrice), cancellationToken);
        
        if (affectedRows == 0)
        {
            throw new ProductNotFoundException(id);
        }
    }

    public async Task<Product> RemoveStockAsync(
        int id,
        int quantityDesired,
        CancellationToken cancellationToken = default)
    {
        var product = await context.Products.FindAsync([id], cancellationToken: cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(id);
        }

        if (quantityDesired <= 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(quantityDesired);
        }

        if (product.AvailableStock == 0)
        {
            throw new ProductOutOfStockException(id);
        }

        if (product.AvailableStock < quantityDesired)
        {
            throw new ProductNotEnoughStockException(id, product.AvailableStock, quantityDesired);
        }

        product.AvailableStock -= quantityDesired;
        await context.SaveChangesAsync(cancellationToken);
        return product.MapToReadModel();
    }

    public async Task<Product> AddStockAsync(
        int id,
        int quantity,
        CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([id], cancellationToken: cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(id);
        }

        if (product.AvailableStock + quantity > product.MaxStockThreshold)
        {
            throw new ProductMaxStockThresholdReachedException(
                id,
                product.MaxStockThreshold,
                product.AvailableStock,
                quantity);
        }

        product.AvailableStock += quantity;
        product.OnReorder = false;
        await context.SaveChangesAsync(cancellationToken);
        return product.MapToReadModel();
    }
}
