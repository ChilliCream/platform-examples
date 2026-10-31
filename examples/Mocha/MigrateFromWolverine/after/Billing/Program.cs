using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Outbox;
using Mocha.Transport.RabbitMQ;
using Shop.Billing;

var builder = WebApplication.CreateBuilder(args);

// Store invoices and Mocha's durable messages in PostgreSQL.
var database = builder.Configuration.GetConnectionString("billing-db")!;
builder.Services.AddDbContext<BillingDb>(x => x.UseNpgsql(database));
builder.AddRabbitMQClient("rabbitmq");

// Configure durable messaging and route confirmations back to Ordering.
builder.Services
    .AddMessageBus()
    .AddBilling()
    .AddEntityFramework<BillingDb>(p =>
    {
        p.UsePostgresOutbox();
        p.UseTransaction();
        p.UsePostgresInbox();
    })
    .AddRabbitMQ();

var app = builder.Build();

// This example creates its business tables without migrations.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDb>().Database.EnsureCreatedAsync();
}

// Expose a health endpoint for the service.
app.MapGet("/health", () => Results.Ok());

await app.RunAsync();
