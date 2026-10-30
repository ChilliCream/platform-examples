var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres").AddDatabase("orders");

builder.AddProject<Projects.MigrateFromMediator_After_Api>("api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
