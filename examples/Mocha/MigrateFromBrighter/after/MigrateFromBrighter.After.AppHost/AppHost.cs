// Define the shared broker and databases for both services.
var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();
var postgres = builder.AddPostgres("postgres");
var orderingDb = postgres.AddDatabase("ordering-db");
var billingDb = postgres.AddDatabase("billing-db");

// Start each service after its broker and database are ready.
builder.AddProject<Projects.MigrateFromBrighter_After_Ordering>("ordering")
    .WithReference(rabbit)
    .WithReference(orderingDb)
    .WaitFor(rabbit)
    .WaitFor(orderingDb);

builder.AddProject<Projects.MigrateFromBrighter_After_Billing>("billing")
    .WithReference(rabbit)
    .WithReference(billingDb)
    .WaitFor(rabbit)
    .WaitFor(billingDb);

builder.Build()
    .Run();
