# Migrate from NServiceBus to Mocha

This example uses Ordering and Billing services with RabbitMQ and PostgreSQL. Ordering saves an order and publishes an event through the transactional outbox. Billing creates an invoice and answers a request for its status. The scenario also demonstrates retry and delayed redelivery, a validation failure, a saga with confirmation and timeout, and a delayed reminder.

## Prerequisites

Install the .NET SDK from the repository's `global.json` and a container runtime supported by Aspire. NServiceBus is free for development but requires a license in production.

## Run the NServiceBus application

From the repository root, run:

```bash
dotnet run --project examples/Mocha/MigrateFromNServiceBus/before/MigrateFromNServiceBus.Before.AppHost --launch-profile https
```

Aspire starts RabbitMQ, PostgreSQL, Ordering and Billing. Use the requests in `scenario.http` against `http://localhost:41310`. Wait for the invoice before requesting its status, and wait 30 seconds to observe the expired order. The reminder is delivered after 10 seconds. Each run uses fresh containers and the fixed IDs in the HTTP file are intended for one run.
