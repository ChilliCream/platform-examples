# Migrate from Mediator

This example starts with an ASP.NET Core API using Mediator 3.0.2 and EF Core. Ordering and Billing are separate class libraries whose handlers are registered by the one mediator generated in the API project. It covers a command with a response, a command without one, a query, a notification with two handlers, pipeline behaviors, pre- and post-processors, an exception processor, and concurrent notification publishing.

`PlaceOrder` and `GetOrder` use Mediator's `IRequest<T>` and `CancelOrder` uses `ICommand`. The command behaviors validate and wrap commands in an ambient transaction. Mediator and its handlers use the default singleton lifetime, so the handlers create and dispose EF Core contexts through `IDbContextFactory` rather than taking a scoped context. Notification handlers log concurrently and do not share a context. The notifications are in process, not durable messages, and there is no RabbitMQ broker or error queue in this example.

## Prerequisites

- The .NET SDK pinned by the repository's `global.json`.
- Docker running locally for the PostgreSQL container started by Aspire.
- An HTTP client that supports `.http` files, or `curl`.

## Run the Mediator app

From the repository root, run:

```bash
dotnet run --project examples/Mocha/MigrateFromMediator/before/MigrateFromMediator.Before.AppHost
```

Aspire starts PostgreSQL and the API. The API listens at `http://localhost:41103`. Run the requests in `scenario.http` in order, or use the returned order ID in subsequent requests. The Aspire dashboard URL is printed at startup. Its resource logs show the pipeline and notification handlers.

PostgreSQL is ephemeral: stopping the AppHost removes its container and data. The example uses `EnsureCreated` at startup rather than EF migrations.
