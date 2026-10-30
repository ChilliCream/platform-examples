using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Outbox;
using Mocha.Scheduling;
using Mocha.Transport.RabbitMQ;
using Shop.Billing;

var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDbContext<BillingDbContext>("billing-db", settings => settings.DisableRetry = true);
builder.AddRabbitMQClient("rabbitmq");
builder.Services
    .AddMessageBus()
    .AddBilling()
    .AddEntityFramework<BillingDbContext>(p =>
    {
        p.UsePostgresOutbox();
        p.UseTransaction();
        p.UsePostgresInbox();
        p.UsePostgresScheduling();
    })
    .AddRabbitMQ(rabbit => rabbit.Handler<ProcessInvoiceConsumer>().ConfigureEndpoint(endpoint =>
        endpoint.AddResilience(policy =>
        {
            policy.On<ValidationException>().DeadLetter();
            policy.Default()
                .Retry([TimeSpan.Zero, TimeSpan.Zero])
                .ThenRedeliver([TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Scenario:RedeliveryDelaySeconds"))]);
        })));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDbContext>().Database.EnsureCreatedAsync();
}
app.MapGet("/", () => "Billing");
app.Run();
