using ChilliCream.Nitro.Aspire;

// Configure the application resources.
var builder = DistributedApplication.CreateBuilder(args);

// Provision PostgreSQL for the ordering service.
var database = builder.AddPostgres("postgres")
    .AddDatabase("ordering-db");

// Start the API after its database is ready.
var ordering = builder.AddProject<Projects.MigrateFromMediatR_After_Ordering>("ordering")
    .WithReference(database)
    .WaitFor(database);

// Collect the ordering service's Nitro resources for the visualizer.
builder.AddNitroResources(port: 41098)
    .WithReference(ordering);

// Start the application.
await builder.Build().RunAsync();
