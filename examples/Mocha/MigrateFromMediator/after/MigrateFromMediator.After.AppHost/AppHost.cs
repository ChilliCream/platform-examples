// Configure the application and its database resource.
var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres")
    .AddDatabase("orders");

// Wait for PostgreSQL before starting the API.
builder.AddProject<Projects.MigrateFromMediator_After_Api>("api")
    .WithReference(database)
    .WaitFor(database);

// Start the distributed application.
builder.Build().Run();
