# Migrate from MassTransit to Mocha

This example migrates an Ordering API and a Billing service from MassTransit 8.x to Mocha. Both apps use RabbitMQ and PostgreSQL through Aspire. Ordering stores an order and publishes `OrderPlaced` through a transactional outbox. Billing creates an invoice, answers a request for its status, and demonstrates retries, delayed redelivery, and a validation error. A saga tracks confirmation or expiry, and a reminder is scheduled for later delivery.

Follow the [MassTransit migration guide](https://chillicream.com/docs/mocha/migration/coming-from-masstransit) alongside the two apps.

## Prerequisites

- Docker running locally.
- The .NET SDK specified in the repository root `global.json`.
- An HTTP client that can run `scenario.http`, or an equivalent client such as curl.

MassTransit 8.5.12 is Apache 2.0 licensed and does not require a license key. The example uses the repository's centrally pinned Mocha version.

## Run the apps

From the repository root, run the MassTransit app:

```bash
dotnet run --project examples/Mocha/MigrateFromMassTransit/before/AppHost --launch-profile https
```

Or run the migrated Mocha app:

```bash
dotnet run --project examples/Mocha/MigrateFromMassTransit/after/AppHost --launch-profile https
```

Aspire starts both services and creates RabbitMQ and PostgreSQL containers. For `before`, Ordering listens on `http://localhost:41210` and the Aspire dashboard is at `https://localhost:41200`. For `after`, Ordering listens on `http://localhost:41260` and the dashboard is at `https://localhost:41250`. Aspire uses the local development certificate for the dashboard.

Run the requests in `scenario.http` in order. Set `@baseUrl` to the Ordering URL for the app you started and use fresh order IDs if the database already contains those IDs. The first order is initially `Submitted` and becomes `Accepted` after Billing confirms its invoice. The second becomes `Expired` after 30 seconds. The transient processing command is attempted three times, then scheduled for redelivery after 3 seconds and succeeds. The validation command is attempted once and sent to `process-invoice_error`. The reminder arrives after 10 seconds. Inspect the service logs and resource connection details in the Aspire dashboard. Wait for invoice creation before making an invoice-status request, and wait for the configured delays before checking the final saga states.

## What changed

The numbered items correspond to the migration guide's steps.

1. Replace the MassTransit and Quartz packages with Mocha, its analyzer, RabbitMQ transport, and PostgreSQL EF Core persistence packages.
2. Replace `AddMassTransit` and its RabbitMQ configuration with `AddMessageBus`, generated service registrations, and `AddRabbitMQ`. Aspire supplies the RabbitMQ connection factory.
3. Convert consumers to event, command, and request handlers. The process handler reads retry metadata from the consume context accessor.
4. Replace publish, send, and request-client calls with `PublishAsync`, `SendAsync`, and `RequestAsync`.
5. Replace retry and scheduled-redelivery middleware with exception policies on the Billing process endpoint. The before app uses Quartz-backed `UseScheduledRedelivery` instead of `UseDelayedRedelivery`, so RabbitMQ does not require the delayed-message exchange plugin. Validation failures go directly to the error endpoint. Ordering configures its own retry policy.
6. Replace the EF Core bus outbox and consumer inbox with Mocha's PostgreSQL outbox, inbox, and transaction middleware. HTTP-originated sends and publishes use an explicit transaction on the same DbContext as the business write.
7. Replace the EF-backed MassTransit state machine with a Mocha saga and PostgreSQL saga store. Correlated messages implement `ICorrelatable`. The `OrderStates` business table keeps the status after Mocha removes a final saga instance.
8. Replace Quartz scheduling with Mocha's PostgreSQL scheduling store. The reminder and saga timeout use the configured short delays.
9. Aspire displays service logs for both apps. This example has no MassTransit test harness to migrate.

## Behavior and storage differences

The HTTP scenario, business records, and application log messages match. The before app publishes `OrderPlaced` directly from the HTTP endpoint. The after app publishes an uncorrelated `StartOrder` from the endpoint, then publishes `OrderPlaced` from the saga's initial transition in the same transaction as the saga state. Billing can therefore only receive `OrderPlaced` after the saga state is committed. The libraries use different message envelopes, endpoint names, inbox and outbox tables, and error-message metadata, so the two versions cannot exchange messages directly. Drain the MassTransit outbox before switching both services together.

MassTransit retains the `Accepted` and `Expired` saga rows. Mocha removes final saga instances; the migrated app stores the visible status separately in `OrderStates`. Both versions cancel the accepted order's scheduled expiry. The example disables Aspire's EF connection retry strategy in the Mocha services because the Mocha transaction middleware and the HTTP transactions otherwise fail with the retrying strategy. This changes database connection resilience and needs review for a production migration.

The before app uses Quartz's in-memory job store, so scheduled messages are lost if Billing stops before their due time. The after app stores scheduled messages in PostgreSQL. Both apps use temporary containers in this example, so broker and database data are removed when the AppHost stops. Both initialize their example schema with `EnsureCreated`; production applications should use database migrations.
