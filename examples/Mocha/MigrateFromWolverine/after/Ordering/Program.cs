using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Mediator;
using Mocha.Outbox;
using Mocha.Sagas;
using Mocha.Scheduling;
using Mocha.Transport.RabbitMQ;
using Shop.Contracts;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetConnectionString("ordering-db")!;
var scenario = builder.Configuration.GetSection("Scenario").Get<ScenarioOptions>() ?? new ScenarioOptions();
OrderSaga.Timeout = TimeSpan.FromSeconds(scenario.SagaTimeoutSeconds);
builder.Services.AddDbContext<OrderingDb>(x => x.UseNpgsql(database));
builder.Services.Configure<ScenarioOptions>(builder.Configuration.GetSection("Scenario"));
builder.AddRabbitMQClient("rabbitmq");
builder.Services.AddMediator().AddOrdering().UseEntityFrameworkTransactions<OrderingDb>();
builder.Services.AddMessageBus()
    .AddResilience(policy =>
    {
        policy.On<ValidationException>().DeadLetter();
        policy.Default().Retry([TimeSpan.Zero, TimeSpan.Zero])
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
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDb>().Database.EnsureCreatedAsync();
}

app.MapPost("/orders", async (PlaceOrder command, ISender sender, CancellationToken ct) =>
    Results.Ok(await sender.SendAsync(command, ct)));
app.MapGet("/orders/{id:guid}", async (Guid id, OrderingDb db, CancellationToken ct) =>
    await db.Orders.FindAsync([id], ct) is { } order ? Results.Ok(order) : Results.NotFound());
app.MapGet("/orders/{id:guid}/invoice", async (Guid id, IMessageBus bus, CancellationToken ct) =>
    Results.Ok(await bus.RequestAsync(new CheckInvoiceStatus(id), ct)));
app.MapPost("/orders/{id:guid}/reminder", async (Guid id, IMessageBus bus, Microsoft.Extensions.Options.IOptions<ScenarioOptions> options, CancellationToken ct) =>
{
    await bus.ScheduleSendAsync(new PaymentReminder(id), DateTimeOffset.UtcNow.AddSeconds(options.Value.ReminderSeconds), ct);
    return Results.Accepted();
});
app.MapPost("/orders/{id:guid}/fail/{failure}", async (Guid id, string failure, IMessageBus bus, CancellationToken ct) =>
{
    await bus.SendAsync(new FailOrder(id, failure), ct);
    return Results.Accepted();
});
app.MapPost("/orders/{id:guid}/audit", async (Guid id, IMessageBus bus, CancellationToken ct) =>
{
    for (var i = 0; i < 5; i++)
    {
        await bus.SendAsync(new AuditOrder(id), ct);
    }
    return Results.Accepted();
});

await app.RunAsync();
