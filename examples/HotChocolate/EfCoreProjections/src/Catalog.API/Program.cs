using eShop.Catalog.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContextPool<CatalogContext>(
        o => o.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDB")))
    .AddMigration<CatalogContext, CatalogContextSeed>();

builder.Services
    .AddScoped<BrandService>()
    .AddScoped<ProductService>()
    .AddScoped<ProductTypeService>();

builder
    .AddGraphQL()
    .AddCatalogTypes()
    .AddMutationConventions()
    .AddGlobalObjectIdentification()
    .AddPagingArguments()
    .AddQueryContext()
    .AddSorting()
    .AddFiltering()
    .ModifyPagingOptions(o =>
    {
        o.EnableRelativeCursors = true;
        o.IncludeTotalCount = true;
    })
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true);

var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);