# PostgresTransport

Demonstrates Mocha's PostgreSQL transport: message publish, send, and
request/reply patterns backed by a Postgres-based message queue, wired up
with .NET Aspire.

## Architecture

The AppHost provisions a single Postgres instance with a shared
`messaging-db` database and starts three services against it:
`OrderService`, `ShippingService`, and `NotificationService`. `OrderService`
exposes demo HTTP endpoints that publish `OrderPlacedEvent`, send
`ProcessOrderCommand` and `OrderShippedEvent`, and issue a
`GetOrderStatusRequest`/response round trip. `ShippingService` and
`NotificationService` subscribe to those events and log the resulting
notifications. `Contracts` holds the shared message types, and
`ServiceDefaults` wires up service discovery, resilience, and OpenTelemetry
for each service.

## Prerequisites

- .NET SDK (see `global.json` at the repository root)
- Docker (for the Postgres container provisioned by Aspire)

## Running

From the repository root:

```bash
dotnet run --project examples/Mocha/PostgresTransport/PostgresTransport.AppHost
```

This starts the Aspire dashboard, provisions the Postgres container, and
launches `OrderService`, `ShippingService`, and `NotificationService`. Use
the dashboard link printed in the console to open the Aspire dashboard and
inspect the running resources, logs, and traces.

Once `OrderService` is running, exercise the demo endpoints, for example:

```bash
curl -X POST http://localhost:<order-service-port>/api/demo/publish
curl -X POST http://localhost:<order-service-port>/api/demo/send
curl -X POST http://localhost:<order-service-port>/api/demo/request-reply
```

Replace `<order-service-port>` with the port shown for `order-service` in the
Aspire dashboard.
