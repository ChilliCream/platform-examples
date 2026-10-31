using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Outbox;
using Mocha.Sagas;
using Mocha.Scheduling;
using Mocha.Transport.RabbitMQ;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);

// Store orders, saga state, and outgoing messages in PostgreSQL.
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db", settings => settings.DisableRetry = true);

// Register the saga, request client, scheduler, and transactional outbox.
builder.AddRabbitMQClient("rabbitmq");
builder.Services
    .AddMessageBus()
    // Retry saga messages before routing failures to the error endpoint.
    .AddResilience(policy => policy.Default()
        .Retry([TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200)])
        .ThenDeadLetter())
    .AddOrdering()
    .AddEntityFramework<OrderingDbContext>(p =>
    {
        p.UsePostgresOutbox();
        p.UseTransaction();
        p.UsePostgresInbox();
        p.AddPostgresSagas();
        p.UsePostgresScheduling();
    })
    .AddRabbitMQ();

var app = builder.Build();

// Create the example schema before receiving requests or messages.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}

// Expose the Ordering API.
app.MapOrderEndpoints();

app.Run();
