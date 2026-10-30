# Migrate from MassTransit to Mocha

This example starts an Ordering API and a Billing service with RabbitMQ and PostgreSQL. The `before` app uses MassTransit 8.5, the EF Core outbox and inbox, a state machine saga, and Quartz scheduling. It is a starting point for the [MassTransit migration guide](https://chillicream.com/docs/mocha/migration/coming-from-masstransit).

## Prerequisites

Install the .NET SDK pinned by the repository, Docker, and a REST client that can run `scenario.http`. MassTransit 8.x is Apache 2.0 licensed and does not require a license key.

## Run the MassTransit app

From the repository root, run:

```bash
dotnet run --project examples/Mocha/MigrateFromMassTransit/before/AppHost --launch-profile https
```

Aspire starts both services and creates RabbitMQ and PostgreSQL containers. The Ordering API listens on `http://localhost:41210`, and the Aspire dashboard is at `https://localhost:41200`.

Run the requests in `scenario.http` in order. Use fresh order IDs for each run. Billing creates an invoice for each order. The confirmed order moves to `Accepted`, and the other expires after 30 seconds. The transient processing command retries and is redelivered, while the validation failure goes to `process-invoice_error`. The reminder is delivered after 10 seconds. View logs and resource connection details in the Aspire dashboard.

Quartz uses its in-memory job store for this local demonstration, so scheduled messages are lost if Billing stops before their due time. RabbitMQ and PostgreSQL use temporary containers, so their data is removed when the AppHost stops.
