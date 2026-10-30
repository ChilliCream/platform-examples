# Migrate from MediatR to the Mocha mediator

This example starts with an ASP.NET Core ordering API using MediatR 12.5 and EF Core with PostgreSQL. It demonstrates commands with and without a response, a query, two notification handlers, request behaviors, pre- and post-processors, and exception handling. MediatR 12 uses the Apache 2.0 license and does not require a license key.

The notification publisher uses `TaskWhenAllPublisher`. MediatR request behaviors do not run for notifications, so the API logs the notification at the publish call. Its two handlers use only loggers and do not share a `DbContext`. The mediator is in-process; there is no message broker, outbox, or error queue.

## Prerequisites

- The .NET SDK specified in the repository's `global.json`.
- Docker with a running daemon, for the PostgreSQL container started by Aspire.
- A .NET HTTPS development certificate, for the Aspire dashboard and resource endpoints.
- An HTTP client. `scenario.http` uses named response variables supported by the VS Code REST Client extension.

## Run the MediatR app

From the repository root, run:

```sh
dotnet run --project examples/Mocha/MigrateFromMediatR/before/MigrateFromMediatR.Before.AppHost --launch-profile https
```

Wait for the ordering service to start, then run the requests in `scenario.http` in order. The API listens at `http://localhost:41003`; the Aspire dashboard is at `https://localhost:41000`. The first request supplies the order ID for subsequent requests. Override `baseUrl` to run against a different listener. The database is created on startup, and the Aspire container is ephemeral.

The invalid command returns 400 without writing an order. A missing query returns 404 via the MediatR exception handler; cancelling a missing order returns 404 after the exception action logs it. Observe request, transaction, processor, and notification logs in the ordering service output in the dashboard. The example disables Aspire's EF Core retries so its explicit command transactions can run without a retrying execution strategy.

## Mocha diagnostics

After migration, the Mocha source generator reports `MO0001` as a warning when a message has no handler and `MO0002` as an error when more than one handler handles a command or query. Fix these diagnostics before running the migrated app.
