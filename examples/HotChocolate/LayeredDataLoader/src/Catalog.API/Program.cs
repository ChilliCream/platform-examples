using eShop.Catalog.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDataLayer(builder.Configuration.GetConnectionString("CatalogDB"))
    .AddServiceLayer()
    .AddMigration<CatalogContext, CatalogContextSeed>();

builder
    .AddGraphQL()
    .AddCatalogTypes()
    .AddPagingArguments()
    .AddQueryContext()
    .AddSorting()
    .AddFiltering()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true);

var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);