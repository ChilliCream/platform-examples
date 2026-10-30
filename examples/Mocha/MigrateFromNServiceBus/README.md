# Migrate from NServiceBus to Mocha

This example migrates two services, Ordering and Billing, from NServiceBus 10.2 to Mocha. Ordering stores an order and publishes `OrderPlaced` through a transactional outbox. Billing creates an invoice and answers a request for its status. The scenario also covers retry and delayed redelivery, a validation failure, an order saga with confirmation and timeout, and a delayed reminder.

## Prerequisites

- Docker, running locally, for RabbitMQ and PostgreSQL.
- The .NET SDK specified in the repository's `global.json`, and a trusted ASP.NET Core development certificate for the Aspire dashboard.
- An HTTP client that supports `scenario.http`, or another client such as curl.

NServiceBus is free for development and needs a license in production. The AppHosts reference the centrally pinned MessagePack 2.5.303 to address a vulnerability in an older transitive dependency.

## Run the applications

From the repository root, start the NServiceBus application:

```bash
dotnet run --project examples/Mocha/MigrateFromNServiceBus/before/MigrateFromNServiceBus.Before.AppHost --launch-profile https
```

Or start the Mocha application:

```bash
dotnet run --project examples/Mocha/MigrateFromNServiceBus/after/MigrateFromNServiceBus.After.AppHost --launch-profile https
```

Each AppHost starts RabbitMQ, PostgreSQL, Ordering, and Billing. Wait for the resources to become healthy in the Aspire dashboard. Stop the AppHost with Ctrl+C when finished.

Open `scenario.http` and run its requests in order. The file defaults to the before application at `http://localhost:41310`; change `@baseUrl` to `http://localhost:41360` for after. The file uses fixed IDs, so start with fresh containers for another run. Wait for the first order to become `Accepted` and for its invoice to exist before requesting the invoice status. Submit the second order without confirmation and wait at least 30 seconds, then read it until it becomes `Expired`. The reminder is scheduled for 10 seconds; watch the Ordering logs. For retry and error details, inspect the Ordering logs and RabbitMQ queues from the Aspire dashboard.

## What changed

The numbered steps follow the [NServiceBus migration guide](https://chillicream.com/docs/mocha/migration/coming-from-nservicebus).

1. **Packages:** Replace NServiceBus, its RabbitMQ transport, SQL persistence, callbacks, and transactional-session packages with Mocha, its analyzer, RabbitMQ transport, and PostgreSQL Entity Framework integration.
2. **Bus:** Replace endpoint configuration and explicit command routes with `AddMessageBus`, generated handler registrations, and RabbitMQ conventions. Aspire supplies the connection factory.
3. **Handlers:** Replace `IHandleMessages<T>` and NServiceBus marker interfaces with Mocha event, command, and request handlers. Request messages specify their response type.
4. **Calls:** Replace session and handler-context calls with `IMessageBus` publish, send, and `RequestAsync` calls. Billing returns the invoice status directly instead of using callbacks.
5. **Recoverability:** Ordering uses one immediate retry, one delayed redelivery, and a validation exception policy that dead-letters without retry. Billing explicitly configures the five immediate and three delayed retries used by NServiceBus defaults. Mocha 16.7.3 requires omitting `ThenDeadLetter()` after `ThenRedeliver()` for redelivery to run.
6. **Outbox and inbox:** Replace SQL persistence and transactional sessions with PostgreSQL EF Core outbox, inbox, and transactions. In the Mocha app, the order insert and internal saga-start command share an explicit transaction; the saga state and `OrderPlaced` publication share the receive transaction. Ordering permits independent sends outside a transaction; handler transactions still use the outbox. Aspire's EF retry strategy is disabled because these examples begin transactions explicitly.
7. **Saga:** Replace saga data, message mappings, and timeout handlers with a persistent state machine. An internal start command creates a saga using the order ID, and its initial transition publishes `OrderPlaced`. `InvoiceConfirmed` supplies the correlation ID and is handled with `OnSend`. The transition updates the order through the scoped DbContext. The timeout is configured before saga registration.
8. **Delayed delivery:** Replace delayed send options with `ScheduleSendAsync` and the PostgreSQL scheduling store for RabbitMQ.
9. **Observability and tests:** Use the Aspire dashboard to inspect service logs and resources. This example has no separate automated test project.

## Behavior differences

The business responses, log messages, and eventual order and invoice states are the same. Processing and delayed delivery are asynchronous, so a status read may still return `Submitted` while confirmation is in flight. Retry and scheduling times are not exact; wait for the expected state or log rather than relying on a fixed instant.

The NServiceBus app publishes `OrderPlaced` in the order transaction and starts the saga from that event. The Mocha app sends an internal start command in the order transaction, then publishes `OrderPlaced` from the saga's initial transition in the same transaction as its state. Billing can therefore confirm only after the saga state has committed.

The message envelopes, headers, exchange and queue names, and persistence schemas differ. NServiceBus uses a shared `error` queue and RabbitMQ delay queues; Mocha uses a separate error queue per receive endpoint, such as `try-invalid_error`, and stores scheduled messages in PostgreSQL. NServiceBus and Mocha cannot consume each other's messages; migrate the communicating flow together and drain pending outbox messages before switching. Mocha cancels a saga deadline on completion, whereas NServiceBus can leave an already-scheduled timeout in its delay queues until delivery.
