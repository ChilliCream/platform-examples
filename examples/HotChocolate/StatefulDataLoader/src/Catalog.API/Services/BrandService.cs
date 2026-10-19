using GreenDonut.Data;
using HotChocolate.Execution;

namespace eShop.Catalog.Services;

public class BrandService(
    CatalogContext context,
    IBrandByIdDataLoader brandById)
{
    public async Task<Brand?> GetBrandByIdAsync(
        int id,
        int tenantId,
        CancellationToken cancellationToken = default)
        => await brandById.WithTenantId(tenantId).LoadAsync(id, cancellationToken);

    public async Task<Page<Brand>> GetBrandsAsync(
        PagingArguments pagingArgs,
        CancellationToken cancellationToken = default)
        => await context.Brands
            .MapToBrand()
            .OrderBy(b => b.Name)
            .ThenBy(t => t.Id)
            .ToPageAsync(pagingArgs, cancellationToken);
}

public static class TenantDataLoaderStateExtensions
{
    public static IDataLoader<TKey, TValue> WithTenantId<TKey, TValue>(
        this IDataLoader<TKey, TValue> dataloader,
        int tenantId)
        where TKey : notnull

    {
        return (IDataLoader<TKey, TValue>)dataloader.Branch(
            tenantId.ToString(),
            CreateBranch,
            tenantId);
    }

    private static IDataLoader CreateBranch<TKey, TValue>(
        string branchKey,
        IDataLoader<TKey, TValue> dataLoader,
        int tenantId)
        where TKey : notnull
    {
        var branch = new BranchedDataLoader<TKey,TValue>(
            (DataLoaderBase<TKey,TValue>)dataLoader,
            branchKey);

        branch.SetState("TenantId", tenantId);

        return branch;
    }

}