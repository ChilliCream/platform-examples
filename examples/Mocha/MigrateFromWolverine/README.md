# Migrate from Wolverine to Mocha

This example migrates two services from Wolverine 6.x to Mocha using the
[Wolverine migration guide](https://chillicream.com/docs/mocha/migration/coming-from-wolverine).
Ordering stores an order and publishes `OrderPlaced` through a transactional
outbox. Billing creates an invoice and can confirm it to an order saga. The
scenario also exercises request/reply, a saga timeout, scheduling, batching,
retries, delayed redelivery, and a validation failure.

## Prerequisites

- Docker with permission to start containers.
- The .NET SDK selected by the repository's `global.json` and the .NET 10 runtime.
- An HTTPS development certificate for the Aspire dashboard.

## Run the applications

Run one of these commands from the repository root. Each AppHost starts Ordering,
Billing, RabbitMQ, and PostgreSQL.

Wolverine:

```bash
dotnet run --project examples/Mocha/MigrateFromWolverine/before/AppHost/MigrateFromWolverine.Before.AppHost.csproj --launch-profile https
```

Mocha:

```bash
dotnet run --project examples/Mocha/MigrateFromWolverine/after/AppHost/MigrateFromWolverine.After.AppHost.csproj --launch-profile https
```

The Wolverine dashboard uses port 41400 and Ordering uses port 41410. The Mocha
dashboard uses port 41450 and Ordering uses port 41460. Stop the AppHost to
remove its ephemeral containers and data.

Open `scenario.http` in an HTTP client that supports named requests and response
variables, such as the VS Code REST Client extension. For Mocha, change `baseUrl`
to `http://localhost:41460`. Run the requests in order. Wait for Billing to
confirm the first order before checking it, then wait at least 30 seconds after
submitting the unconfirmed order before checking its status. The reminder delay
is 10 seconds, and the redelivery delay is 2 seconds. Polling and processing can
add time. Inspect the logs and RabbitMQ management UI through the Aspire
dashboard.

## Structure

Each side has the same projects and folders:

- `AppHost/` starts the services and their infrastructure.
- `Contracts/` contains the messages exchanged between services and the
  Wolverine timeout message. The Wolverine project references `WolverineFx`
  because that timeout derives from `TimeoutMessage`.
- `Ordering/Commands/` contains `PlaceOrderCommand` and its local handler.
  `Ordering/Endpoints/`, `Ordering/Handlers/`, `Ordering/Sagas/`, and
  `Ordering/Data/` hold the HTTP routes, bus handlers, saga, and EF Core model.
  `Ordering/Services/` contains the simulated dependency used for retries.
- `Billing/Handlers/` handles orders and invoice status requests, and
  `Billing/Data/` contains the invoice model and context.

## The saga

Wolverine's `before/Ordering/Sagas/OrderSaga.cs` persists the saga itself and
returns a timeout message. Mocha's `after/Ordering/Sagas/OrderSaga.cs` configures
transitions over the separate `OrderState.cs` and publishes `OrderPlaced` from
its initial transition. Both update the order when confirmation arrives or the
saga times out.

## What changed

The numbered items correspond to the migration guide.

1. Replace Wolverine packages with Mocha, its analyzer, RabbitMQ, mediator, and
   PostgreSQL persistence packages. Both AppHosts reference MessagePack 2.5.303
   to avoid a vulnerable transitive version from Aspire.
2. Replace Wolverine host configuration with the generated Mocha registration
   and an Aspire-provided RabbitMQ connection factory.
3. Replace convention handlers and method injection with Mocha handler
   interfaces and constructor injection. Explicitly publish `InvoiceConfirmed`
   instead of returning it as a cascading message.
4. Move the local `PlaceOrderCommand` invocation to the Mocha mediator, including its
   Entity Framework transaction middleware.
5. Replace remote `InvokeAsync<InvoiceStatus>` with `RequestAsync` and update
   publish and send calls. The `OrderPlaced` and `InvoiceConfirmed` contracts
   and HTTP routes remain unchanged.
6. Configure validation failures to dead-letter immediately and other failures
   to retry twice, then redeliver once after the configured delay.
7. Configure the PostgreSQL outbox, inbox, and handler transactions on each
   service's `DbContext`. The mediator transaction includes the order and its
   outgoing saga-start event. The saga's initial transition publishes
   `OrderPlaced` in the same transaction as its state, before Billing can
   confirm it. Independent endpoint sends allow an outbox without an existing
   transaction.
8. Replace the convention saga and `TimeoutMessage` with a Mocha state machine
   and PostgreSQL saga and scheduling stores. The saga uses the scoped ordering
   context in `OnEnterStateAsync` to update the order during its transition.
9. Replace the array batch handler with `IBatchEventHandler` and use
   `ScheduleSendAsync` for the reminder.
10. Remove Wolverine runtime compilation and generated-code configuration from
    the migrated application. Mocha generates registrations at build time.

## Behavior differences

The applications expose the same scenario responses and application log
messages, but their RabbitMQ queue names, message envelopes, and PostgreSQL
persistence tables differ. Drain Wolverine's pending messages before switching:
Mocha cannot consume Wolverine envelopes or its persisted messages. Wolverine
uses one dead-letter queue here, while Mocha uses an error queue per receiving
endpoint. Both leave the validation failure in an error queue.

Wolverine retries an event for an unknown saga through its error policies.
Mocha 16.7.3 ignores an event for a saga that does not exist yet. The Mocha
example commits saga creation and the `OrderPlaced` outbox message together so
Billing cannot produce a confirmation before the saga exists.

Both sides omit the earlier application log that reported framework retry
attempts. The simulated order service fails three times and then recovers; use
the framework logs and queue outcomes to observe retries and redelivery.

Wolverine's remote invocation times out after five seconds by default; Mocha's
request waits up to two minutes unless the caller cancels it. Delivery and
scheduled-message polling can also change observed timing. Mocha 16.7.3 needs
`Retry().ThenRedeliver()` without a trailing `ThenDeadLetter()` for delayed
redelivery in this example. Both versions use the same configured scenario
delays. The example does not demonstrate recovery after a crash, rollback under
an injected failure, or duplicate delivery.
