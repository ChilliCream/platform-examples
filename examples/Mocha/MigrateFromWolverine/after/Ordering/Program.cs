using System.ComponentModel.DataAnnotations;
using ChilliCream.Nitro;
using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Mediator;
using Mocha.Outbox;
using Mocha.Sagas;
using Mocha.Scheduling;
using Mocha.Transport.RabbitMQ;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);

// Configure the scenario and its simulated external dependency.
var database = builder.Configuration.GetConnectionString("ordering-db")!;
var scenario = builder.Configuration.GetSection("Scenario").Get<ScenarioOptions>() ?? new ScenarioOptions();
OrderSaga.Timeout = TimeSpan.FromSeconds(scenario.SagaTimeoutSeconds);
builder.Services.Configure<ScenarioOptions>(builder.Configuration.GetSection("Scenario"));
builder.Services.AddSingleton<SimulatedOrderService>();

// Store orders and Mocha's durable messages in PostgreSQL.
builder.Services.AddDbContext<OrderingDb>(x => x.UseNpgsql(database));
builder.AddRabbitMQClient("rabbitmq");

// Export messaging and mediator resources for the visualizer.
builder.Services.AddNitro().AddMocha().AddMochaMediator();

// The mediator runs the local command in an EF transaction.
builder.Services
    .AddMediator()
    .AddOrdering()
    .UseEntityFrameworkTransactions<OrderingDb>();

// Configure durable messaging, batching, and exception policies.
builder.Services
    .AddMessageBus()
    .AddResilience(policy =>
    {
        // Validation fails immediately; transient failures retry and then redeliver.
        policy.On<ValidationException>().DeadLetter();
        policy.Default()
            .Retry([TimeSpan.Zero, TimeSpan.Zero])
            .ThenRedeliver([TimeSpan.FromSeconds(scenario.RetryDelaySeconds)]);
    })
    .AddOrdering()
    .AddBatchHandler<AuditOrderHandler>(batch =>
    {
        batch.MaxBatchSize = 5;
        batch.BatchTimeout = TimeSpan.FromSeconds(scenario.BatchTimeoutSeconds);
    })
    .AddEntityFramework<OrderingDb>(p =>
    {
        p.UsePostgresOutbox(o => o.RequireTransaction = false);
        p.UseTransaction();
        p.UsePostgresInbox();
        p.AddPostgresSagas();
        p.UsePostgresScheduling();
    })
    .AddRabbitMQ();

var app = builder.Build();

// This example creates its business tables without migrations.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDb>().Database.EnsureCreatedAsync();
}

// Expose the same HTTP scenario before and after migration.
app.MapOrderEndpoints();

await app.RunAsync();
