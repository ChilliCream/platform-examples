using eShop.Catalog.Services;
using eShop.Catalog.Services.Errors;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace eShop.Catalog.Types;

[MutationType]
public static partial class ProductMutations
{
    [Error<ProductNotFoundException>]
    public static async Task<Product> RenameProductAsync(
        int id,
        string newName,
        QueryContext<Product> query,
        ProductService productService,
        CancellationToken ct)
    {
        await productService.RenameProductAsync(id, newName, ct);
        return (await productService.GetProductByIdAsync(id, query, ct))!;
    }

    [Mutation]
    [Error<ProductNotFoundException>]
    [Error<ProductPriceCannotBeNegative>]
    public static async Task<Product> ChangeProductPriceAsync(
        int id,
        decimal newPrice,
        QueryContext<Product> query,
        ProductService productService,
        CancellationToken ct)
    {
        await productService.ChangeProductPriceAsync(id, newPrice, ct);
        return (await productService.GetProductByIdAsync(id, query, ct))!;
    }

    [Mutation]
    [Error<ProductOutOfStockException>]
    [Error<ProductNotEnoughStockException>]
    public static async Task<Product> AddProductStockAsync(
        int id,
        int quantity,
        ProductService productService,
        CancellationToken ct)
        => await productService.AddStockAsync(id, quantity, ct);

    [Mutation]
    [Error<ProductOutOfStockException>]
    [Error<ProductNotEnoughStockException>]
    public static async Task<Product> RemoveProductStockAsync(
        int id,
        int quantityDesired,
        ProductService productService,
        CancellationToken ct)
        => await productService.RemoveStockAsync(id, quantityDesired, ct);
}