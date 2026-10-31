using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts;
using Shop.Ordering;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Configure the scenario and its simulated external dependency.
var database = builder.Configuration.GetConnectionString("ordering-db")!;
var scenario = builder.Configuration.GetSection("Scenario").Get<ScenarioOptions>() ?? new ScenarioOptions();
builder.Services.Configure<ScenarioOptions>(builder.Configuration.GetSection("Scenario"));
builder.Services.AddSingleton<SimulatedOrderService>();

// Store orders and Wolverine's durable messages in PostgreSQL.
builder.Services.AddDbContextWithWolverineIntegration<OrderingDb>(x => x.UseNpgsql(database));

// Configure durable messaging, routing, batching, and exception policies.
builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithPostgresql(database, "wolverine");
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.AutoApplyTransactions();
    opts.Policies.UseDurableLocalQueues();
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    opts.Policies.UseDurableInboxOnAllListeners();
    opts.UseRabbitMq(builder.Configuration.GetConnectionString("rabbitmq")!)
        .AutoProvision();

    opts.ListenToRabbitQueue("wolverine-ordering");
    opts.PublishMessage<OrderSubmitted>().ToRabbitQueue("wolverine-ordering");
    opts.PublishMessage<InvoiceConfirmed>().ToRabbitQueue("wolverine-ordering");
    opts.PublishMessage<FailOrder>().ToRabbitQueue("wolverine-ordering");
    opts.PublishMessage<AuditOrder>().ToRabbitQueue("wolverine-ordering");
    opts.PublishMessage<OrderPlaced>().ToRabbitQueue("wolverine-billing");
    opts.PublishMessage<CheckInvoiceStatus>().ToRabbitQueue("wolverine-billing");

    opts.BatchMessagesOf<AuditOrder>(batch =>
    {
        batch.BatchSize = 5;
        batch.TriggerTime = TimeSpan.FromSeconds(scenario.BatchTimeoutSeconds);
    });

    // Validation fails immediately; transient failures retry and then redeliver.
    opts.Policies.OnException<ValidationException>().MoveToErrorQueue();
    opts.Policies.OnAnyException()
        .RetryTimes(2)
        .Then.ScheduleRetry(TimeSpan.FromSeconds(scenario.RetryDelaySeconds))
        .Then.MoveToErrorQueue();
});

var app = builder.Build();

// This example creates its business tables without migrations.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDb>().Database.EnsureCreatedAsync();
}

// Expose the same HTTP scenario before and after migration.
app.MapOrderEndpoints();

await app.RunAsync();
