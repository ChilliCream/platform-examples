using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Scheduling;
using Mocha.Transport.RabbitMQ;
using Npgsql;
using Shop.Billing;

var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetConnectionString("billing-db")!;
builder.Services.AddSingleton(NpgsqlDataSource.Create(database));
builder.AddNpgsqlDbContext<BillingDbContext>("billing-db");
builder.AddRabbitMQClient("rabbitmq");
builder.Services.AddMessageBus()
    .AddResilience(policy =>
    {
        policy.On<ValidationException>().DeadLetter();
        policy.On<TransientException>().Retry(1, TimeSpan.FromMilliseconds(100)).ThenRedeliver(3, TimeSpan.FromSeconds(1));
    })
    .AddBilling()
    .AddEntityFramework<BillingDbContext>(p => p.UsePostgresScheduling())
    .AddRabbitMQ();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDbContext>().Database.EnsureCreatedAsync();
}
await BillingDatabase.InitializeAsync(app.Services.GetRequiredService<NpgsqlDataSource>());
app.MapGet("/health", () => Results.Ok());
await app.RunAsync();
