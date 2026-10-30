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
dotnet run --project examples/Mocha/MigrateFromMediator/before/MigrateFromMediator.Before.AppHost --launch-profile https
```

Aspire starts PostgreSQL and the API. The API listens at `http://localhost:41103`. Run the requests in `scenario.http` in order, or use the returned order ID in subsequent requests. The Aspire dashboard URL is printed at startup. Its resource logs show the pipeline and notification handlers.

PostgreSQL is ephemeral: stopping the AppHost removes its container and data. The example uses `EnsureCreated` at startup rather than EF migrations.

## Run the Mocha app

From the repository root, run:

```bash
dotnet run --project examples/Mocha/MigrateFromMediator/after/MigrateFromMediator.After.AppHost --launch-profile https
```

The API listens at `http://localhost:41153`. In `scenario.http`, change `@baseUrl` to that address, then run the requests in order. An HTTP client with named-request support can substitute the `placeOrder` response automatically. Otherwise, copy the returned `orderId` into the subsequent requests. The requests cover placing and reading an order, invalid input, a missing order, and cancellation.

## What changed

The [Mediator migration guide](https://chillicream.com/docs/mocha/migration/coming-from-mediator) describes the steps shown by `after/`:

1. Install `Mocha.Mediator` and `Mocha.Analyzers` in the handler projects. The generator emits a registration method per project.
2. Change the namespace to `Mocha.Mediator`. `PlaceOrder` becomes `ICommand<Guid>`, `GetOrder` becomes `IQuery<OrderDto?>`, and `CancelOrder` remains `ICommand`.
3. Replace request handlers with command or query handlers, rename `Handle` to `HandleAsync`, and return `ValueTask` instead of `ValueTask<Unit>` for `CancelOrder`.
4. Register the generated `AddOrdering()` and `AddBilling()` modules with `AddMediator()`. Mocha uses scoped handlers instead of Mediator's singleton handlers. Both versions retain the context factory and ambient transaction to keep the database code the same.
5. Dispatch commands with `SendAsync`, queries with `QueryAsync`, and notifications with `PublishAsync`.
6. Replace the logging, validation, and transaction behaviors with middleware. Filters keep command-only middleware on order commands and exclude notifications from request logging.
7. Replace the preprocessor, postprocessor, and exception processor with middleware that preserves their order and exception handling.
8. Configure `NotificationPublishMode.Concurrent` in place of `TaskWhenAllPublisher`.
9. Remove `Mediator.SourceGenerator` and `Mediator.Abstractions` from the migrated app.

Both apps reference the centrally pinned patched `MessagePack` dependency in their Aspire AppHosts to avoid a transitive vulnerability warning.

The guide's staged migration also works with both libraries registered side by side: use a namespace alias for Mocha message types and call `Mocha.Mediator.MediatorServiceCollectionExtensions.AddMediator` explicitly. Mediator behaviors and processors do not run for messages dispatched through Mocha, so migrate those concerns with each message.

## Behavior differences

For the supplied scenario, the HTTP responses, application log messages, and observable order states match apart from generated order IDs. Logger categories change with the middleware class names. The two notification handlers run concurrently, so their log order is not guaranteed. These are in-process notifications, not durable messages; there are no queues or error queue. In sequential notification mode, Mediator runs remaining handlers after a failure and aggregates exceptions, while Mocha stops at the first failure. This example uses concurrent mode on both sides.
