using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Mediator;
using Mocha.Outbox;
using Mocha.Sagas;
using Mocha.Scheduling;
using Mocha.Transport.RabbitMQ;
using Shop.Ordering;

// Configure the service and its database.
var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");
builder.AddRabbitMQClient("rabbitmq");

// Register the in-process mediator and the message bus with PostgreSQL persistence.
builder.Services
    .AddMediator()
    .AddOrdering();
builder.Services
    .AddMessageBus()
    .AddOrdering()
    .AddEntityFramework<OrderingDbContext>(p =>
    {
        p.UsePostgresOutbox(o => o.RequireTransaction = false);
        p.AddPostgresSagas();
        p.UsePostgresScheduling();
        p.UseResilience();
        p.UseTransaction();
    })
    .AddRabbitMQ();

// Create the business and Mocha tables before the bus starts.
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}

// Map the HTTP API and start the service.
app.MapOrderEndpoints();
await app.RunAsync();
