# Migrate from Brighter

This example uses Brighter 10.8.1 to run an Ordering service and a Billing service over RabbitMQ. Ordering sends an in-process command, stores an order and deposits `OrderPlaced` into a PostgreSQL outbox in the same transaction. The outbox sweeper publishes it. Billing creates an invoice and can confirm the order. Ordering can also call Billing for the invoice status, schedule a reminder, and exercise retry, redelivery, and validation failure handling.

Brighter has no saga abstraction. The before app tracks the order process in a PostgreSQL table and updates it from message handlers. The Mocha migration replaces this process with a saga. An unconfirmed order expires after 30 seconds, and a reminder is sent after 10 seconds. These delays are configured in the Ordering app's `appsettings.json`. Brighter's default in-memory scheduler is used for delayed posts; pending delayed messages do not survive a service restart.

A validation failure is acknowledged and dropped by Brighter when the handler neither requeues nor rejects it. Mocha sends an unhandled failed message to the endpoint's error queue instead. The transient failure example retries once in process and then defers the message for redelivery; it succeeds on the third attempt.

## Prerequisites

- The .NET SDK pinned in the repository's `global.json` and a trusted ASP.NET Core development certificate.
- Docker running locally. Aspire starts RabbitMQ and PostgreSQL with dynamic container ports.
- An HTTP client that supports `.http` files, or curl.

The AppHost references the centrally pinned MessagePack 2.5.303 to avoid a vulnerability in an older transitive dependency. All package versions are in the root `Directory.Packages.props`.

## Run the Brighter app

From the repository root, run:

```bash
dotnet run --project examples/Mocha/MigrateFromBrighter/before/MigrateFromBrighter.Before.AppHost --launch-profile https
```

Open the Aspire dashboard using the login URL printed by the AppHost. Leave `@baseUrl` in `scenario.http` set to `http://localhost:41510`.

## Run the Mocha app

From the repository root, run:

```bash
dotnet run --project examples/Mocha/MigrateFromBrighter/after/MigrateFromBrighter.After.AppHost --launch-profile https
```

Open the Aspire dashboard using the login URL printed by the AppHost. Set `@baseUrl` in `scenario.http` to `http://localhost:41560`.

## Drive the scenario

Run the requests in `scenario.http` in order. The first request returns an order ID, and the named requests use it for follow-up calls. Wait for Billing to confirm the first order before checking its status. Check the second order both before and after the 30-second timeout. Both orders produce reminders after 10 seconds.

Inspect the Ordering service's `orders`, `order_processes`, and `reminders` tables and the Billing service's `invoices` and `attempts` tables. The service logs show the submitted, accepted, expired, and reminder events, invoice creation, three transient attempts, and the validation failure. In RabbitMQ, Brighter acknowledges and drops the invalid message, while Mocha leaves it in `exercise-validation-failure_error`.

## What changed

The [Brighter migration guide](https://chillicream.com/docs/mocha/migration/coming-from-brighter) describes these steps:

1. Install the Mocha mediator, bus, analyzer, RabbitMQ, and PostgreSQL EF Core packages in place of the Brighter packages.
2. Sort in-process handlers from subscription handlers. `PlaceOrder` runs in process, while the other messages travel over RabbitMQ.
3. Replace the producer registry, subscriptions, mappers, and service activator with generated mediator and bus registrations and RabbitMQ conventions.
4. Replace the command processor's `SendAsync` and `RequestHandlerAsync<PlaceOrder>` with `ISender` and `ICommandHandler<PlaceOrder>`.
5. Replace subscription handlers with bus handlers. In the Mocha app, an order saga replaces the Brighter confirmation and expiry handlers; it persists its state in PostgreSQL and projects the terminal status to `order_processes`.
6. Replace `PostAsync` with bus sends or publishes, and replace Brighter `Call` with `IMessageBus.RequestAsync`.
7. Replace the Polly resilience pipeline and deferred requeue attribute with Mocha exception policies. Validation failures go directly to the error queue; transient failures retry once and can be redelivered.
8. Replace `DepositPostAsync` and the outbox sweeper with the Mocha PostgreSQL outbox. The mediator writes the order and sends `StartOrder` in one transaction. The saga publishes `OrderPlaced` through its own transactional outbox after it starts.
9. Replace delayed posts with scheduled sends and the PostgreSQL scheduling store. The saga requests expiry scheduling after it has started, using the deadline set when the order was submitted.
10. Use the service logs and Aspire dashboard to inspect the flows. The example does not include a test project.

## Behavior differences

- Brighter drops the validation failure because the handler has no requeue or reject policy. Mocha keeps the failed message and exception details in `exercise-validation-failure_error`.
- Brighter's in-memory scheduler loses pending delayed messages on restart. Mocha stores scheduled messages in PostgreSQL.
- In the example, Brighter's `Call` leaves a response in its generated reply queue after returning. Mocha consumes the response from its reply endpoint. The two libraries also use different queue and exchange names and cannot exchange envelopes directly.
- Mocha deletes saga state on completion. The separate `order_processes` table retains the accepted or expired status for the HTTP endpoint.
