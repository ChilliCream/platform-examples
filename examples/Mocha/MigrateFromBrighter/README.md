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

Open the Aspire dashboard using the login URL printed by the AppHost. Run the requests in `scenario.http` in order. The first request returns an order ID, and the named requests use it for the follow-up calls. The second order does not receive an invoice confirmation, so check it both before and after the timeout. The `reminders` and `order_processes` tables in the Ordering database and the `invoices` and `attempts` tables in the Billing database show the effects of the handlers. Service logs and RabbitMQ queues are available through the Aspire resources.
