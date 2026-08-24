# Demo

An e-commerce order/payment/shipping walkthrough built with Mocha, wired up
with .NET Aspire. It demonstrates events, commands, request/reply,
per-message and batch event handlers side by side, sagas coordinating
multi-step flows across services, and the Postgres outbox/inbox with
RabbitMQ transport.

## Architecture

The AppHost provisions a shared RabbitMQ instance and a shared Postgres
server (one database per service: `catalog-db`, `billing-db`,
`shipping-db`), then starts three services: `Catalog`, `Billing`, and
`Shipping`. `Catalog` owns products and orders, exposes the `PlaceOrderCommand`,
`PlaceBulkOrderCommand`, and `RequestQuickRefundCommand`/`InitiateReturnCommand`
sagas (`QuickRefundSaga`, `ReturnProcessingSaga`) that coordinate refund and
return flows with `Billing` and `Shipping` via request/reply. `Billing`
handles `OrderPlacedEvent` with both a per-message handler
(`OrderPlacedEventHandler`, one invoice per order) and a batch handler
(`OrderPlacedBatchHandler`/`BulkOrderBatchHandler`, aggregating a batch into
a single `RevenueSummary` row), and processes payments and refunds.
`Shipping` creates and ships shipments and receives returned packages.
`Contracts` holds the shared message types, and `ServiceDefaults` wires up
service discovery, resilience, and OpenTelemetry for each service.

## Prerequisites

- .NET SDK (see `global.json` at the repository root)
- Docker (for the RabbitMQ and Postgres containers provisioned by Aspire)

## Running

From the repository root:

```bash
dotnet run --project examples/Mocha/Demo/Demo.AppHost
```

This starts the Aspire dashboard, provisions RabbitMQ and Postgres, and
launches `Catalog`, `Billing`, and `Shipping`. Use the dashboard link
printed in the console to open the Aspire dashboard and inspect the running
resources, logs, and traces, and to find each service's port.

The AppHost also exposes two infrastructure web UIs through the Aspire
dashboard's resource list:

- **RabbitMQ management UI** (`rabbitmq`, via `WithManagementPlugin()`):
  inspect exchanges, queues, and in-flight messages.
- **pgweb** (`postgres`, via `WithPgWeb()`): browse the `catalog-db`,
  `billing-db`, and `shipping-db` tables.

Each service also exposes a small REST API for driving the demo directly:

| Service    | Endpoints                                                                                                                                                                                                             |
| ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Catalog`  | `GET /`, `GET /api/products`, `GET /api/products/{id}`, `GET /api/categories`, `POST /api/orders`, `POST /api/orders/bulk`, `GET /api/orders`, `GET /api/orders/{id}`, `POST /api/refunds/quick`, `POST /api/returns/initiate` |
| `Billing`  | `GET /`, `GET /api/invoices`, `GET /api/invoices/{id}`, `GET /api/invoices/order/{orderId}`, `POST /api/payments/{invoiceId}`, `GET /api/payments`, `GET /api/refunds`, `GET /api/refunds/order/{orderId}`, `GET /api/revenue-summaries`, `GET /api/revenue-summaries/latest` |
| `Shipping` | `GET /`, `GET /api/shipments`, `GET /api/shipments/{id}`, `GET /api/shipments/order/{orderId}`, `POST /api/shipments/{id}/ship`, `GET /api/returns`, `GET /api/returns/{id}`, `GET /api/returns/order/{orderId}`, `POST /api/returns/{id}/receive` |

### Using the `.http` files

`http/` contains [VS Code REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
scripts that drive the services end to end:

| File                             | Description                                                    |
| --------------------------------- | ---------------------------------------------------------------- |
| `config.http`                     | Shared service URLs and sample product IDs                     |
| `01-place-single-order.http`      | Basic order flow: place order → payment → shipment             |
| `02-quick-refund.http`            | Quick refund saga (no physical return)                         |
| `03-return-processing.http`       | Full return saga with inspection and parallel processing       |
| `04-batch-processing.http`        | Per-message and batch handlers both reacting to `OrderPlacedEvent` |
| `05-high-volume-batch.http`       | Thousands of bulk order events processed in size-based batches |

1. Open any `.http` file in VS Code with the REST Client extension installed.
2. Update the `@catalogUrl`, `@billingUrl`, `@shippingUrl` variables to match
   the ports shown in the Aspire dashboard.
3. Click "Send Request" above each request, in order, waiting a couple of
   seconds between steps for async processing. Variables such as `@orderId`
   are captured from responses and reused by later requests in the same file.

## Event Flow

```
┌─────────────┐     OrderPlacedEvent      ┌─────────────┐
│   Catalog   │ ─────────────────────────▶│   Billing   │
│   Service   │                           │   Service   │
└─────────────┘                           └─────────────┘
       ▲                                         │
       │                                         │
       │    ShipmentCreatedEvent                 │ PaymentCompletedEvent
       │         ShipmentShippedEvent             │
       │                                         ▼
       │                                  ┌─────────────┐
       └──────────────────────────────────│   Shipping  │
                                          │   Service   │
                                          └─────────────┘
```
