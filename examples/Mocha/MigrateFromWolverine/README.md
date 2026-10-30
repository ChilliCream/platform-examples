# Migrate from Wolverine to Mocha

The `before` application uses Wolverine with RabbitMQ and PostgreSQL. Ordering
stores orders and publishes `OrderPlaced` through a transactional outbox. Billing
creates invoices and confirms them to an order saga. The scenario also covers
request/reply, delayed messages, batch handling, retries, redelivery, and a
validation failure.

## Prerequisites

- The .NET SDK specified in the repository's `global.json` and the .NET 10 runtime.
- Docker with permission to start containers.
- An HTTPS development certificate for the Aspire dashboard.

## Run the Wolverine application

From the repository root:

```bash
dotnet run --project examples/Mocha/MigrateFromWolverine/before/AppHost/MigrateFromWolverine.Before.AppHost.csproj --launch-profile https
```

The Aspire dashboard uses port 41400 and Ordering uses port 41410. Use the
requests in `scenario.http` in order against Ordering. Wait for Billing to confirm
the invoice before checking the first order. Wait for the configured 30-second
saga timeout before checking the unconfirmed order, and 10 seconds for the
reminder. The dashboard shows the application logs and RabbitMQ management UI.
The Aspire resources are ephemeral, so their data is removed when the AppHost
stops.
