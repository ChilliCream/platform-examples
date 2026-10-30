using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Scheduling;
using Mocha.Transport.RabbitMQ;
using Shop.Billing;

// Configure the service and its database.
var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDbContext<BillingDbContext>("billing-db");
builder.AddRabbitMQClient("rabbitmq");

// Validation failures go to the error queue; transient failures retry and redeliver.
builder.Services.AddMessageBus()
    .AddResilience(policy =>
    {
        policy.On<ValidationException>().DeadLetter();
        policy.On<TransientException>()
            .Retry(1, TimeSpan.FromMilliseconds(100))
            .ThenRedeliver(3, TimeSpan.FromSeconds(1));
    })
    .AddBilling()
    .AddEntityFramework<BillingDbContext>(p => p.UsePostgresScheduling())
    .AddRabbitMQ();

// Create the business and Mocha tables before the bus starts.
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDbContext>().Database.EnsureCreatedAsync();
}

// Map the health endpoint and start the service.
app.MapGet("/health", () => Results.Ok());
await app.RunAsync();
