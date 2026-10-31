using ChilliCream.Nitro.Aspire;

// Configure the application and its database resource.
var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres")
    .AddDatabase("orders");

// Wait for PostgreSQL before starting the API.
var api = builder.AddProject<Projects.MigrateFromMediator_After_Api>("api")
    .WithReference(database)
    .WaitFor(database);

// Collect the API's mediator resources for the Nitro visualizer.
builder.AddNitroResources(port: 41198)
    .WithReference(api);

// Start the distributed application.
builder.Build().Run();
