using System.ComponentModel.Design;
using eShop.Catalog.Services;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContextPool<CatalogContext>(
        o => o.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDB")))
    .AddMigration<CatalogContext, CatalogContextSeed>();

builder.Services
    .AddScoped<BrandService>()
    .AddScoped<ProductService>();

builder
    .AddGraphQL()
    .AddCatalogTypes()
    .AddPagingArguments()
    .AddQueryContext()
    .AddSorting()
    .AddFiltering()
    .ModifyPagingOptions(o => o.EnableRelativeCursors = true)
    .AddHttpRequestInterceptor<CustomHttpRequestInterceptor>()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true);

builder.Services
    .AddSingleton<IPromiseCacheInterceptor, CustomPromiseCacheInterceptor>();


var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);

public class CustomPromiseCacheInterceptor : IPromiseCacheInterceptor
{
    public Promise<T> GetOrAddPromise<T>(
        PromiseCacheKey key,
        Func<PromiseCacheKey, Promise<T>> createPromise)
    {
        Console.WriteLine($"------>>>{key.Type}:{key.Key}<<<------");
        return createPromise(key);
    }

    public bool TryAdd<T>(PromiseCacheKey key, Promise<T> promise)
    {
        return false;
    }
}

public class CustomHttpRequestInterceptor : DefaultHttpRequestInterceptor
{
    public override ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor,
        OperationRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {

        requestBuilder.SetGlobalState("TenantId", int.Parse(context.Request.Headers["tenant-id"].First()!));
        return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}
