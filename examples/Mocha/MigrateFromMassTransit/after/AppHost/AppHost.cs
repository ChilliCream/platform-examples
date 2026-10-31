using ChilliCream.Nitro.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder.AddRabbitMQ("rabbitmq").WithManagementPlugin();
var postgres = builder.AddPostgres("postgres");
var orderingDb = postgres.AddDatabase("ordering-db");
var billingDb = postgres.AddDatabase("billing-db");

var ordering = builder.AddProject<Projects.MigrateFromMassTransit_After_Ordering>("ordering")
    .WithReference(rabbit)
    .WithReference(orderingDb)
    .WaitFor(rabbit)
    .WaitFor(orderingDb);

var billing = builder.AddProject<Projects.MigrateFromMassTransit_After_Billing>("billing")
    .WithReference(rabbit)
    .WithReference(billingDb)
    .WaitFor(rabbit)
    .WaitFor(billingDb);

// Collect the messaging resources exported by both services.
builder.AddNitroResources(port: 41298)
    .WithReference(ordering)
    .WithReference(billing);

builder.Build().Run();
