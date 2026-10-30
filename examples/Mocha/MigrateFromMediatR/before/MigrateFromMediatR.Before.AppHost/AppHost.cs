var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres").AddDatabase("ordering-db");

builder.AddProject<Projects.MigrateFromMediatR_Before_Ordering>("ordering")
    .WithReference(database)
    .WaitFor(database);

await builder.Build().RunAsync();
