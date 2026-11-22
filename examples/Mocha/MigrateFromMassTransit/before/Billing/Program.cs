using System.ComponentModel.DataAnnotations;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Shop.Billing;

var builder = WebApplication.CreateBuilder(args);

// Store invoices and outgoing messages in PostgreSQL.
builder.AddNpgsqlDbContext<BillingDbContext>("billing-db");
builder.Services.AddSingleton<InvoiceProcessor>();

// Quartz delivers scheduled confirmations and redeliveries.
builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

// Register consumers and the transactional outbox.
var rabbit = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!);
var credentials = rabbit.UserInfo.Split(':', 2);
builder.Services.AddMassTransit(x =>
{
    x.AddPublishMessageScheduler();
    x.AddQuartzConsumers();
    x.AddConsumer<OrderPlacedConsumer>();
    x.AddConsumer<CheckInvoiceStatusConsumer>();
    x.AddConsumer<SendReminderConsumer>();
    x.AddConsumer<ProcessInvoiceConsumer>()
        .Endpoint(e => e.Name = "process-invoice");
    x.AddEntityFrameworkOutbox<BillingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    // Retry transient processing failures, then redeliver; validation failures go to the error queue.
    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        if (name == "process-invoice")
        {
            cfg.UseScheduledRedelivery(r =>
            {
                r.Ignore<ValidationException>();
                r.Intervals(TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Scenario:RedeliveryDelaySeconds")));
            });
            cfg.UseMessageRetry(r =>
            {
                r.Ignore<ValidationException>();
                r.Immediate(2);
            });
        }
        if (name is "OrderPlaced" or "CheckInvoiceStatus" or "SendReminder" or "process-invoice")
        {
            cfg.UseEntityFrameworkOutbox<BillingDbContext>(context);
        }
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

// Create the example schema before receiving messages.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDbContext>().Database.EnsureCreatedAsync();
}

// Expose the service endpoint.
app.MapGet("/", () => "Billing");

app.Run();
