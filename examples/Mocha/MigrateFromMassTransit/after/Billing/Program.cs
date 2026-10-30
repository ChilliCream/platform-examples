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

// Store invoices and outgoing messages in PostgreSQL.
builder.AddNpgsqlDbContext<BillingDbContext>("billing-db", settings => settings.DisableRetry = true);
builder.Services.AddSingleton<InvoiceProcessor>();

// Register consumers, scheduling, and the transactional outbox.
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
    // Retry transient processing failures, then redeliver; validation failures go to the error queue.
    .AddRabbitMQ(rabbit => rabbit.Handler<ProcessInvoiceConsumer>().ConfigureEndpoint(endpoint =>
        endpoint.AddResilience(policy =>
        {
            policy.On<ValidationException>().DeadLetter();
            policy.Default()
                .Retry([TimeSpan.Zero, TimeSpan.Zero])
                .ThenRedeliver(
                    [TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Scenario:RedeliveryDelaySeconds"))]);
        })));

var app = builder.Build();

// Create the example schema before receiving messages.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDbContext>().Database.EnsureCreatedAsync();
}

// Expose the service endpoint.
app.MapGet("/", () => "Billing");

app.Run();
