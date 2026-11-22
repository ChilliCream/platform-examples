# Migrate from MediatR to the Mocha mediator

This example starts with an ASP.NET Core ordering API using MediatR 12.5 and EF Core with PostgreSQL. It demonstrates commands with and without a response, a query, two notification handlers, request behaviors, pre- and post-processors, and exception handling. MediatR 12 uses the Apache 2.0 license and does not require a license key.

The notification publisher uses `TaskWhenAllPublisher`. MediatR request behaviors do not run for notifications, so the API logs the notification at the publish call. Its two handlers use only loggers and do not share a `DbContext`. The mediator is in-process; there is no message broker, outbox, or error queue. The AppHosts reference MessagePack 2.5.303 in `before/` and 2.5.307 in `after/` to avoid a vulnerability warning and satisfy the Nitro dependency.

## Structure

Each side is a standalone Aspire application with the same `Shop.Ordering` namespace. The AppHost starts PostgreSQL and the ordering API. In each ordering project:

- `Commands/` contains `PlaceOrderCommand` and `CancelOrderCommand` with their handlers.
- `Queries/` contains `GetOrderQuery`, its result types, and its handler.
- `Notifications/` contains `OrderPlacedNotification` and both notification handlers.
- `Data/` contains the EF Core context and order entity; `Endpoints/` maps the HTTP API.
- `Behaviors/` in `before/` contains the MediatR behaviors, processors, and exception handlers. `Middleware/` in `after/` contains the corresponding Mocha middleware.

The logged message type names include the `Command`, `Query`, or `Notification` suffix used by the example's types. Both applications write the same business log messages.

## Prerequisites

- The .NET SDK specified in the repository's `global.json`.
- Docker with a running daemon, for the PostgreSQL container started by Aspire.
- A .NET HTTPS development certificate, for the Aspire dashboard and resource endpoints.
- An HTTP client. `scenario.http` uses named response variables supported by the VS Code REST Client extension.

## Run the apps

From the repository root, start the MediatR app:

```sh
dotnet run --project examples/Mocha/MigrateFromMediatR/before/MigrateFromMediatR.Before.AppHost --launch-profile https
```

Or start the migrated Mocha app:

```sh
dotnet run --project examples/Mocha/MigrateFromMediatR/after/MigrateFromMediatR.After.AppHost --launch-profile https
```

Wait for the ordering service to start. The MediatR API listens at `http://localhost:41003` and its Aspire dashboard is at `https://localhost:41000`. The Mocha API listens at `http://localhost:41053` and its dashboard is at `https://localhost:41050`. Each AppHost starts an ephemeral PostgreSQL container and creates the database on startup. Stop the AppHost to stop its resources.

In `after/`, the `nitro-resources` resource in the Aspire dashboard has a Visualizer link.

Run the requests in `scenario.http` in order using the VS Code REST Client extension. For the Mocha app, change `baseUrl` in that file to `http://localhost:41053`. The first request supplies the order ID to the later requests. The invalid command returns 400 without writing an order. A missing query returns 404 through the exception handler; cancelling a missing order returns 404 after the exception action logs it. Observe request, transaction, processor, and notification logs in the ordering service output in the dashboard. Both apps disable Aspire's EF Core retries so their explicit command transactions can run without a retrying execution strategy.

## Migration steps

The [MediatR migration guide](https://chillicream.com/docs/mocha/migration/coming-from-mediatr) describes the changes shown in `after/`:

1. Install `Mocha.Mediator` and `Mocha.Analyzers`.
2. Replace `IRequest<Guid>` and `IRequest` with `ICommand<Guid>` and `ICommand`, and `IRequest<OrderResult>` with `IQuery<OrderResult>`. Keep `OrderPlacedNotification` as a notification in the Mocha namespace.
3. Implement the corresponding Mocha handler interfaces and rename `Handle` to `HandleAsync`, returning `ValueTask`.
4. Replace `AddMediatR` and assembly scanning with `AddMediator().AddOrdering()`, where `AddOrdering` is generated.
5. Replace `Send` with `SendAsync` for commands and `QueryAsync` for queries, and replace `Publish` with `PublishAsync`.
6. Replace logging, validation, and transaction behaviors with middleware. The factories filter the message types each middleware handles.
7. Replace pre- and post-processors and exception handler and action with middleware around `next`, including a fallback result for a missing order.
8. Select `NotificationPublishMode.Concurrent` to match `TaskWhenAllPublisher`.
9. Remove the MediatR dependency from `after/`.

## Behavior differences

The scenario has the same response bodies and status codes, application log messages, and order rows in both apps, except for generated order IDs. Notification handler order is not guaranteed with concurrent publishing. The sample handlers complete synchronously, so their logs do not demonstrate asynchronous overlap. Mocha registers handlers as scoped by default; MediatR registers them as transient. The handlers in this sample do not retain state between dispatches.

If multiple notification handlers fail, awaiting MediatR's `TaskWhenAllPublisher` normally surfaces one exception, while Mocha's concurrent publisher surfaces an `AggregateException`. The scenario's notification handlers do not throw. Neither app has a message broker or an error queue.

## Mocha diagnostics

The Mocha source generator reports `MO0001` as a warning when a command or query has no handler and `MO0002` as an error when more than one handler handles a command or query. Notifications can have multiple handlers. Fix these diagnostics before running the migrated app.
