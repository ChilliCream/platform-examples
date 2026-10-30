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
    .Host(h => h.ServiceName("billing"))
    .AddBilling()
    .AddEntityFramework<BillingDbContext>(p =>
    {
        p.UsePostgresOutbox();
        p.UseTransaction();
        p.UsePostgresInbox();
        p.UsePostgresScheduling();
    })
    .AddRabbitMQ();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    await db.Database.MigrateAsync();
}
app.Run();
