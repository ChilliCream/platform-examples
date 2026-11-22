using Microsoft.EntityFrameworkCore;
using Shop.Billing;
using Shop.Contracts;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Store invoices and Wolverine's durable messages in PostgreSQL.
var database = builder.Configuration.GetConnectionString("billing-db")!;
builder.Services.AddDbContextWithWolverineIntegration<BillingDb>(x => x.UseNpgsql(database));

// Configure durable messaging and route confirmations back to Ordering.
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

    opts.ListenToRabbitQueue("wolverine-billing");
    opts.PublishMessage<InvoiceConfirmed>()
        .ToRabbitQueue("wolverine-ordering");
});

var app = builder.Build();

// This example creates its business tables without migrations.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDb>().Database.EnsureCreatedAsync();
}

// Expose a health endpoint for the service.
app.MapGet("/health", () => Results.Ok());

await app.RunAsync();
