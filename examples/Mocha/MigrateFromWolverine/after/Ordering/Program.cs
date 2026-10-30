using Microsoft.EntityFrameworkCore;
using Shop.Contracts;
using Shop.Ordering;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetConnectionString("ordering-db")!;
var scenario = builder.Configuration.GetSection("Scenario").Get<ScenarioOptions>() ?? new ScenarioOptions();
builder.Services.AddDbContextWithWolverineIntegration<OrderingDb>(x => x.UseNpgsql(database));
builder.Services.Configure<ScenarioOptions>(builder.Configuration.GetSection("Scenario"));

builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithPostgresql(database, "wolverine");
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.AutoApplyTransactions();
    opts.Policies.UseDurableLocalQueues();
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    opts.Policies.UseDurableInboxOnAllListeners();
    opts.UseRabbitMq(builder.Configuration.GetConnectionString("rabbitmq")!).AutoProvision();
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
    opts.Policies.OnException<System.ComponentModel.DataAnnotations.ValidationException>().MoveToErrorQueue();
    opts.Policies.OnAnyException().RetryTimes(2)
        .Then.ScheduleRetry(TimeSpan.FromSeconds(scenario.RetryDelaySeconds))
        .Then.MoveToErrorQueue();
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDb>().Database.EnsureCreatedAsync();
}

app.MapPost("/orders", async (PlaceOrder command, IMessageBus bus, CancellationToken ct) =>
    Results.Ok(await bus.InvokeAsync<Guid>(command, ct)));
app.MapGet("/orders/{id:guid}", async (Guid id, OrderingDb db, CancellationToken ct) =>
    await db.Orders.FindAsync([id], ct) is { } order ? Results.Ok(order) : Results.NotFound());
app.MapGet("/orders/{id:guid}/invoice", async (Guid id, IMessageBus bus, CancellationToken ct) =>
    Results.Ok(await bus.InvokeAsync<InvoiceStatus>(new CheckInvoiceStatus(id), ct)));
app.MapPost("/orders/{id:guid}/reminder", async (Guid id, IMessageBus bus, Microsoft.Extensions.Options.IOptions<ScenarioOptions> options) =>
{
    await bus.ScheduleAsync(new PaymentReminder(id), TimeSpan.FromSeconds(options.Value.ReminderSeconds));
    return Results.Accepted();
});
app.MapPost("/orders/{id:guid}/fail/{failure}", async (Guid id, string failure, IMessageBus bus) =>
{
    await bus.SendAsync(new FailOrder(id, failure));
    return Results.Accepted();
});
app.MapPost("/orders/{id:guid}/audit", async (Guid id, IMessageBus bus) =>
{
    for (var i = 0; i < 5; i++)
    {
        await bus.SendAsync(new AuditOrder(id));
    }
    return Results.Accepted();
});

await app.RunAsync();
