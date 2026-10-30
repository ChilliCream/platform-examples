var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder.AddRabbitMQ("rabbitmq").WithManagementPlugin();
var postgres = builder.AddPostgres("postgres");
var orderingDb = postgres.AddDatabase("ordering-db");
var billingDb = postgres.AddDatabase("billing-db");

builder.AddProject<Projects.MigrateFromWolverine_Before_Ordering>("ordering")
    .WithReference(rabbit)
    .WithReference(orderingDb)
    .WaitFor(rabbit)
    .WaitFor(orderingDb);

builder.AddProject<Projects.MigrateFromWolverine_Before_Billing>("billing")
    .WithReference(rabbit)
    .WithReference(billingDb)
    .WaitFor(rabbit)
    .WaitFor(billingDb);

builder.Build().Run();
