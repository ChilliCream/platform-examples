using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);

// Store orders, saga state, and outgoing messages in PostgreSQL.
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");

// Register the saga, request client, scheduler, and transactional outbox.
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

    // Retry saga messages and persist outgoing messages with their consumer transaction.
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

// Create the example schema before receiving requests or messages.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}

// Expose the Ordering API.
app.MapOrderEndpoints();

app.Run();
