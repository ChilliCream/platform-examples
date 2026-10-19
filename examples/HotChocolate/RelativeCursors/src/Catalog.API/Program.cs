using eShop.Catalog.Services;
using HotChocolate.Types.Pagination;

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
    .ModifyPagingOptions(o => o.EnableRelativeCursors = true);

var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);