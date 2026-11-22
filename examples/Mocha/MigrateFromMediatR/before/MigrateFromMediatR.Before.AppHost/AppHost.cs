// Configure the application resources.
var builder = DistributedApplication.CreateBuilder(args);

// Provision PostgreSQL for the ordering service.
var database = builder.AddPostgres("postgres")
    .AddDatabase("ordering-db");

// Start the API after its database is ready.
builder.AddProject<Projects.MigrateFromMediatR_Before_Ordering>("ordering")
    .WithReference(database)
    .WaitFor(database);

// Start the application.
await builder.Build().RunAsync();
