# Master DataLoader in Layered Architecture

This example shows how to use DataLoader in the business layer without taking a dependency on Hot Chocolate. The services in `Catalog.Services` batch and cache lookups through DataLoaders, and the GraphQL project `Catalog.API` consumes them.

Watch the episode: https://www.youtube.com/watch?v=ZHq1pBjo0Qk
Episode page: https://chillicream.com/videos/2025-03-05-dataloader-in-layered-architecture

Hot Chocolate version: 15.1.0-p.5 (`HotChocolate.AspNetCore`, `HotChocolate.Data.EntityFramework`, and the `GreenDonut` packages). Targets .NET 9.0.

## Projects

```text
src/Catalog.Data      EF Core context, entities, and entity configurations (PostgreSQL)
src/Catalog.Services  Business layer: services and DataLoaders
src/Catalog.API       Hot Chocolate GraphQL server, query types, migrations, and seed data
```

## Prerequisites

- .NET SDK matching the `global.json` of the repository root
- Docker, to run PostgreSQL with `compose.yml`

## Run

Start PostgreSQL (`pgvector` image, port 5432) and pgAdmin (port 5050):

```bash
docker compose up -d
```

The local development credentials in `compose.yml` and `src/Catalog.API/appsettings.json` are `admin` / `root`. They are for local use only.

Run the API:

```bash
dotnet run --project src/Catalog.API
```

The launch profile listens on `http://localhost:5224`. The GraphQL endpoint is `/graphql`. On startup the API applies the EF Core migrations to the `CatalogDB` database and seeds it.

Stop PostgreSQL with:

```bash
docker compose down
```
