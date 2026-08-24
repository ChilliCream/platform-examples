# AotExample

Demonstrates running Mocha under Native AOT: message publish/subscribe and
request/reply over RabbitMQ, a saga tracking an order through shipment, and a
mediator handling in-process commands and queries, all in AOT-compiled and
trim-compatible services.

## Architecture

`OrderService` and `FulfillmentService` are both published with
`PublishAot` and `IsAotCompatible` enabled. `OrderService` hosts a background
worker that places orders through the mediator (`PlaceOrderCommand`,
`GetOrderStatusQuery`), then publishes `OrderPlacedEvent` and
`OrderShippedEvent` to the bus; an `OrderSaga` tracks each order from
placement to shipment with a 30-second timeout. `FulfillmentService`
subscribes to `OrderPlacedEvent`, checks inventory with `OrderService` via a
request/reply call (`CheckInventoryRequest`), and publishes
`OrderShippedEvent` once stock is confirmed. `Contracts` holds the shared
message types and their `JsonSerializerContext` for reflection-free
serialization under AOT.

## Prerequisites

- .NET SDK (see `global.json` at the repository root)
- Docker (for the RabbitMQ container)

## Running

Start RabbitMQ from this directory:

```bash
docker compose up -d
```

Then, from the repository root, run each service in its own terminal:

```bash
dotnet run --project examples/Mocha/AotExample/AotExample.OrderService
dotnet run --project examples/Mocha/AotExample/AotExample.FulfillmentService
```

`OrderService` starts placing simulated orders a few seconds after launch;
watch both services' console output to follow an order through placement,
inventory check, and shipment.

To verify the services build and run under Native AOT, publish them with the
runtime identifier for your machine, for example:

```bash
dotnet publish examples/Mocha/AotExample/AotExample.OrderService -c Release -r linux-x64
dotnet publish examples/Mocha/AotExample/AotExample.FulfillmentService -c Release -r linux-x64
```

The published binaries are self-contained native executables under each
project's `bin/Release/net11.0/<rid>/publish/` folder.

Publishing emits IL2104 and IL3053 trim and AOT warnings for
`Mocha.Transport.RabbitMQ`. These originate in the published 16.6.1 package,
not in this example, and the published binaries run correctly.
