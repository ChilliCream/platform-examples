using System.ComponentModel.DataAnnotations;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Shop.Billing;

var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDbContext<BillingDbContext>("billing-db");
builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var rabbit = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!);
var credentials = rabbit.UserInfo.Split(':', 2);
builder.Services.AddMassTransit(x =>
{
    x.AddPublishMessageScheduler();
    x.AddQuartzConsumers();
    x.AddConsumer<OrderPlacedConsumer>();
    x.AddConsumer<CheckInvoiceStatusConsumer>();
    x.AddConsumer<SendReminderConsumer>();
    x.AddConsumer<ProcessInvoiceConsumer>().Endpoint(e => e.Name = "process-invoice");
    x.AddEntityFrameworkOutbox<BillingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
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
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDbContext>().Database.EnsureCreatedAsync();
}
app.MapGet("/", () => "Billing");
app.Run();
