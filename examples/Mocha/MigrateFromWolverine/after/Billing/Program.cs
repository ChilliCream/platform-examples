using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Outbox;
using Mocha.Transport.RabbitMQ;
using Shop.Billing;

var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetConnectionString("billing-db")!;
builder.Services.AddDbContext<BillingDb>(x => x.UseNpgsql(database));
builder.AddRabbitMQClient("rabbitmq");
builder.Services.AddMessageBus()
    .AddBilling()
    .AddEntityFramework<BillingDb>(p =>
    {
        p.UsePostgresOutbox();
        p.UseTransaction();
        p.UsePostgresInbox();
    })
    .AddRabbitMQ();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDb>().Database.EnsureCreatedAsync();
}
app.MapGet("/health", () => Results.Ok());
await app.RunAsync();
