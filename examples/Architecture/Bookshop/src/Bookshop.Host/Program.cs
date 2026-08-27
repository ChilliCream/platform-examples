using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using Bookshop.GraphQL;
using Bookshop.Host.Host;
using Bookshop.Host.Host.Authorization;
using Bookshop.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Application registers Mocha's command/query handlers and domain services. Infrastructure
// supplies persistence, DataLoader implementations, the unit of work, and command transaction
// middleware. GraphQL reaches them through Application contracts and mediator messages.
builder.Services.AddBookshopApplication();
builder.Services.AddSingleton<IAuthorizationService, AllowAllAuthorizationService>();
builder.Services.AddBookshopInfrastructure(builder.Configuration);
builder.Services.AddBookshopGraphQL();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live", "ready"])
    .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

// Seed data is local-dev convenience only; never runs outside Development. Registered after
// AddBookshopInfrastructure (which registers BookshopMigrationHostedService) so migrations
// always run first.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<BookshopSeedHostedService>();
}

var app = builder.Build();

app.UseRouting();

app.MapBookshopGraphQL();
app.MapHealthChecks("/_health/live", new()
{
    Predicate = check => check.Tags.Contains("live"),
});
app.MapHealthChecks("/_health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready"),
});

return await app.RunWithGraphQLCommandsAsync(args);
