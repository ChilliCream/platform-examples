# ExceptionPolicies

Demonstrates Mocha's per-exception resilience policies: dead-lettering,
discarding, retrying, redelivering, and chaining retry into redelivery into
dead-lettering, including conditional policies keyed on exception state. Uses
the InMemory transport for simplicity, no external dependencies.

## Architecture

A single host registers eight event handlers, each throwing a different
exception to exercise one policy shape. `Program.cs` configures all of them
in one `AddResilience` call: `MessageValidationException` dead-letters
immediately, `DuplicateMessageException` discards, `PaymentGatewayException`
retries with exponential backoff, `AuthTokenExpiredException` redelivers
without retrying, `TransientDatabaseException` retries then redelivers,
`PoisonMessageException` retries once then dead-letters,
`ExternalServiceUnavailableException` chains retry into redelivery into
dead-letter, and `HttpServiceException` branches on its status code (404,
429, 503) to three different policies for the same exception type. A final
`On<Exception>()` catch-all covers anything unmatched.

## Prerequisites

- .NET SDK (see `global.json` at the repository root)

## Running

From the repository root:

```bash
dotnet run --project examples/Mocha/ExceptionPolicies
```

The host starts and registers all eight handlers and their policies; it does
not publish any messages itself, so no handler runs and the process just
idles. Read `Program.cs` alongside `Handlers/Handlers.cs` as a configuration
reference for how each policy shape is declared, or wire a publisher into
`Program.cs` to drive the handlers and observe retries, redeliveries, and
dead-letters in the console output.
