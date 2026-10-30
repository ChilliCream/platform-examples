using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");

var rabbit = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!);
var credentials = rabbit.UserInfo.Split(':', 2);
builder.Services.AddMassTransit(x =>
{
    x.AddPublishMessageScheduler();
    x.AddRequestClient<CheckInvoiceStatus>();
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Optimistic;
            r.ExistingDbContext<OrderingDbContext>();
            r.UsePostgres();
        });
    x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddConfigureEndpointsCallback((context, _, cfg) =>
    {
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromMilliseconds(200)));
        cfg.UseEntityFrameworkOutbox<OrderingDbContext>(context);
    });
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbit.Host, (ushort)rabbit.Port, "/", h =>
        {
            h.Username(Uri.UnescapeDataString(credentials[0]));
            h.Password(Uri.UnescapeDataString(credentials[1]));
        });
        cfg.UsePublishMessageScheduler();
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}

app.MapPost("/orders", async (PlaceOrder request, OrderingDbContext db, IPublishEndpoint publisher, CancellationToken ct) =>
{
    var order = new Order { Id = request.OrderId, Amount = request.Amount, ConfirmInvoice = request.ConfirmInvoice };
    db.Orders.Add(order);
    await publisher.Publish(new OrderPlaced(order.Id, order.Amount, order.ConfirmInvoice), ct);
    await db.SaveChangesAsync(ct);
    app.Logger.LogInformation("Order {OrderId} placed", order.Id);
    return Results.Accepted($"/orders/{order.Id}", new { order.Id });
});

app.MapGet("/orders/{id:guid}", async (Guid id, OrderingDbContext db, CancellationToken ct) =>
{
    var order = await db.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    var saga = await db.OrderStates.AsNoTracking().SingleOrDefaultAsync(x => x.CorrelationId == id, ct);
    return order is null ? Results.NotFound() : Results.Ok(new { order.Id, order.Amount, order.ConfirmInvoice, Saga = saga?.CurrentState });
});

app.MapGet("/orders/{id:guid}/invoice", async (Guid id, IRequestClient<CheckInvoiceStatus> client, CancellationToken ct) =>
{
    var response = await client.GetResponse<InvoiceStatus>(new CheckInvoiceStatus(id), ct);
    return Results.Ok(response.Message);
});

app.MapPost("/orders/{id:guid}/process/{failure}", async (Guid id, string failure, ISendEndpointProvider provider, OrderingDbContext db, CancellationToken ct) =>
{
    var endpoint = await provider.GetSendEndpoint(new Uri("queue:process-invoice"));
    await endpoint.Send(new ProcessInvoice(id, failure), ct);
    await db.SaveChangesAsync(ct);
    return Results.Accepted();
});

app.MapPost("/orders/{id:guid}/reminder", async (Guid id, IMessageScheduler scheduler, IConfiguration configuration, OrderingDbContext db, CancellationToken ct) =>
{
    var delay = configuration.GetValue<int>("Scenario:ReminderDelaySeconds");
    await scheduler.SchedulePublish(DateTime.UtcNow.AddSeconds(delay), new SendReminder(id), ct);
    await db.SaveChangesAsync(ct);
    app.Logger.LogInformation("Reminder for order {OrderId} scheduled", id);
    return Results.Accepted();
});

app.Run();

public record PlaceOrder(Guid OrderId, decimal Amount, bool ConfirmInvoice);
