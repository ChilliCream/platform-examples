var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder.AddRabbitMQ("rabbitmq").WithManagementPlugin();
var postgres = builder.AddPostgres("postgres");
var orderingDb = postgres.AddDatabase("ordering-db");
var billingDb = postgres.AddDatabase("billing-db");

builder.AddProject<Projects.MigrateFromNServiceBus_After_Ordering>("ordering")
    .WithReference(rabbit).WithReference(orderingDb)
    .WithEnvironment("RabbitMqManagementUrl", rabbit.GetEndpoint("management"))
    .WaitFor(rabbit).WaitFor(orderingDb);
builder.AddProject<Projects.MigrateFromNServiceBus_After_Billing>("billing")
    .WithReference(rabbit).WithReference(billingDb)
    .WithEnvironment("RabbitMqManagementUrl", rabbit.GetEndpoint("management"))
    .WaitFor(rabbit).WaitFor(billingDb);

builder.Build().Run();
