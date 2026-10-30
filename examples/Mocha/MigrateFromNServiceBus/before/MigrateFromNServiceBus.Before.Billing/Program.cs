using Npgsql;
using NpgsqlTypes;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using Shop.Contracts;

[assembly: SqlPersistenceSettings(PostgreSqlScripts = true)]

var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetConnectionString("billing-db")!;
var rabbit = builder.Configuration.GetConnectionString("rabbitmq")!;
var endpoint = new EndpointConfiguration("Billing");
endpoint.UseSerialization<SystemJsonSerializer>();
endpoint.EnableInstallers();
endpoint.EnableCallbacks(makesRequests: false);
var rabbitTransport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), rabbit)
{
    ManagementApiConfiguration = new ManagementApiConfiguration(builder.Configuration["RabbitMqManagementUrl"]!)
};
var transport = endpoint.UseTransport(rabbitTransport);
transport.RouteToEndpoint(typeof(InvoiceConfirmed), "Ordering");
var persistence = endpoint.UsePersistence<SqlPersistence>();
var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
dialect.JsonBParameterModifier(p => ((NpgsqlParameter)p).NpgsqlDbType = NpgsqlDbType.Jsonb);
persistence.ConnectionBuilder(() => new NpgsqlConnection(database));
endpoint.EnableOutbox();
builder.Services.AddNServiceBusEndpoint(endpoint);

await using (var connection = new NpgsqlConnection(database))
{
    await connection.OpenAsync();
    await using var command = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS invoices (order_id uuid PRIMARY KEY, status text NOT NULL)", connection);
    await command.ExecuteNonQueryAsync();
}

var app = builder.Build();
app.Run();
