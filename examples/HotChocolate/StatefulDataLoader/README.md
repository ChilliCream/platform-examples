# How to Use State in DataLoader for Context-Aware Fetching

This example shows how to make Hot Chocolate DataLoaders stateful, so they can receive a tenant ID from the request and fetch data in the correct scope without losing batching.

Watch the episode: https://www.youtube.com/watch?v=4Mw2A548OGM
Episode page: https://chillicream.com/videos/2025-05-01-stateful-dataloader

Hot Chocolate version: 15.1.3 (`HotChocolate.AspNetCore`, `HotChocolate.Data.EntityFramework`, `HotChocolate.Types.Analyzers`), .NET 9, EF Core 9 with Npgsql.

## Prerequisites

- .NET SDK (the root `global.json` pins the SDK for the platform-examples repository)
- Docker, to run PostgreSQL with `compose.yml`

## Run

1. Start PostgreSQL and pgAdmin from the example folder:

   ```bash
   docker compose up -d
   ```

   The database is `CatalogDB` on `localhost:5432` with user `admin` and password `root` (local development only).

2. Start the API:

   ```bash
   dotnet run --project src/Catalog.API
   ```

   On startup the app applies the EF Core migrations and seeds the catalog from `src/Catalog.API/Migrations/catalog.json`. The `http` launch profile listens on `http://localhost:5224/`, and the GraphQL endpoint is served at `/graphql`.

3. Send GraphQL requests with the `tenant-id` HTTP header. The header is required, and its value is read as an integer tenant ID.
