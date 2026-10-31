using ChilliCream.Nitro.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder.AddRabbitMQ("rabbitmq").WithManagementPlugin();
var postgres = builder.AddPostgres("postgres");
var orderingDb = postgres.AddDatabase("ordering-db");
var billingDb = postgres.AddDatabase("billing-db");

var ordering = builder.AddProject<Projects.MigrateFromWolverine_After_Ordering>("ordering")
    .WithReference(rabbit)
    .WithReference(orderingDb)
    .WaitFor(rabbit)
    .WaitFor(orderingDb);

var billing = builder.AddProject<Projects.MigrateFromWolverine_After_Billing>("billing")
    .WithReference(rabbit)
    .WithReference(billingDb)
    .WaitFor(rabbit)
    .WaitFor(billingDb);

// Collect the services' messaging resources for the Nitro visualizer.
builder.AddNitroResources(port: 41498)
    .WithReference(ordering)
    .WithReference(billing);

builder.Build().Run();
