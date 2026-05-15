# BrewUpErp

BrewUpErp is an end‑to‑end sample ERP built to demonstrate:

- Domain‑Driven Design (DDD)
- Modular / bounded‑context architecture
- Functional‑style domain modelling
- Clean, testable application layers

The solution is intentionally realistic but still small enough to study.
It consists of:

- **BrewApi** – backend HTTP API (ASP.NET Core, .NET 10)
- **BrewSpa** – Blazor web front‑end
- **BrewApp** – .NET MAUI mobile application

> Note: this repository is a work in progress. Expect breaking changes
> and incomplete features, especially in less‑used flows.

---

## Solution Structure

High‑level folder layout:

```
src/
├── BrewApi/          # Backend API, domain, and infrastructure
├── BrewSpa/          # Blazor web front‑end
└── BrewApp/          # .NET MAUI mobile app
BrewDocs/             # Bruno API contracts & example payloads
docker/               # docker-compose.yml and helper script
```

### BrewApi

The backend solution (`src/BrewApi/BrewUp.slnx`) hosts all domain logic and HTTP endpoints.

**Infrastructure & cross‑cutting projects:**

| Project | Purpose |
|---|---|
| `BrewUp.Rest` | ASP.NET Core Web API host – composition root with explicit module registration |
| `BrewUp.Shared` | Shared abstractions: value objects, domain IDs, messages (commands/events), read‑model contracts, validators, helpers |
| `BrewUp.Infrastructure` | Cross‑cutting infrastructure: MongoDB helper, RabbitMQ settings, read‑model base |
| `BrewUp.AppHost` | .NET Aspire AppHost for local orchestration |
| `BrewUp.ServiceDefaults` | .NET Aspire service defaults (OpenTelemetry, health checks, etc.) |
| `BrewUp.Mediator` | Custom lightweight mediator |
| `BrewUp.Warehouse.Entities` | Shared Warehouse DTO/entity types used across layers |

**Bounded contexts** (each contains `Domain`, `Facade`, `Infrastructure`, `ReadModel`, `SharedKernel`, `Tests`):

| Context | Description |
|---|---|
| `MasterData/` | Beer catalog, customers, and core reference data (also has a `.Entities` project) |
| `Sales/` | Sales orders and related workflows |
| `Purchases/` | Purchase orders |
| `Warehouse/` | Warehouse and stock management |
| `Dashboards/` | Cross‑context dashboard aggregations (also has a `.Entities` project) |
| `Sagas/` | Long‑running process managers / sagas |

**AI & MCP integration:**

| Project | Purpose |
|---|---|
| `Mcp/BrewUp.Mcp.McpServer` | Standalone MCP (Model Context Protocol) server exposing ERP tools over HTTP |
| `Mcp/BrewUp.Mcp.Facade` | AI chat service, MCP tool façades, Azure OpenAI integration endpoints |
| `Mcp/BrewUp.Mcp.SharedKernel` | Shared types for the MCP layer |
| `AI/BrewUp.AI.Facade` | AI façade contracts |
| `AI/BrewUp.AI.SharedKernel` | AI shared kernel types |

**Module composition in `BrewUp.Rest`:**

`Program.cs` uses an explicit composition‑root pattern — each feature registers itself as an `IModule`:

```
CorsModule · LoggingModule · InfrastructureModule · OpenApiModule
MasterDataModule · PurchasesModule · SalesModule · WarehouseModule
DashboardsModule · SagasModule · ChatModule
```

### BrewSpa

The Blazor front‑end solution (`src/BrewSpa/BrewSpa.slnx`) mirrors the API's bounded‑context decomposition:

| Project | Purpose |
|---|---|
| `BrewSpa/BrewSpa` | Main Blazor app – shell, layouts, pages (`Home`, `NotFound`), `wwwroot` |
| `BrewSpa/BrewSpa.Shared` | Shared client models, helpers, and shared tests |
| `BrewSpa/BrewSpa.Shared.Components` | Reusable Razor components, custom types, JS interop, messages |
| `Dashboards/` | `BrewSpa.Dashboards.ApplicationServices`, `BrewSpa.Dashboards.Facade`, `BrewSpa.Dashboards.Tests` |
| `MasterData/` | `BrewSpa.MasterData.Application`, `BrewSpa.MasterData.Facade` |
| `Sales/` | `BrewSpa.Sales.Application`, `BrewSpa.Sales.Facade` |

### BrewApp

The .NET MAUI mobile solution lives under `src/BrewApp/mobile/`:

| Folder | Purpose |
|---|---|
| `mobile/src/` | `BrewApp.Mobile` – `AppShell`, features, components, services, models, platform targets |
| `mobile/tests/` | Mobile UI / integration tests |
| `specs/` | Specification/acceptance tests (`.specify`) |

### Bounded Context Layer Layout

Each bounded context follows the same layered structure:

| Layer | Responsibility |
|---|---|
| `*.Domain` | Aggregates, value objects, domain events, domain services |
| `*.SharedKernel` | Types shared only within the bounded context |
| `*.Infrastructure` | Persistence and integration (MongoDB, event store, messaging) |
| `*.ReadModel` | Denormalized read models and projections |
| `*.Facade` | Application services / use‑case orchestration |

---

## Technology Stack

- **Runtime**: .NET 10 (`net10.0`)
- **API**: ASP.NET Core Web API
  - OpenAPI via Scalar
  - Observability with OpenTelemetry and Azure Monitor exporter
  - Logging with Serilog (console + file sinks)
  - Explicit module composition root pattern
- **Architecture**:
  - DDD with explicit domain and application layers
  - Modular monolith style, grouped by bounded context
  - CQRS and message‑driven patterns powered by **Muflone**
  - Custom lightweight mediator (`BrewUp.Mediator`)
  - .NET Aspire for local orchestration
- **Persistence & Messaging**:
  - MongoDB – read‑model document store (separate instances per context)
  - KurrentDB (EventStoreDB) – event store for domain events and sagas
  - RabbitMQ 4.x with Stream plugin – message bus
- **AI / MCP**:
  - Azure OpenAI integration (chat completions)
  - Model Context Protocol (MCP) server exposing ERP tools to AI agents
- **Web UI**: Blazor (server‑side, `net10.0`)
- **Mobile**: .NET MAUI (Android / iOS / Windows)

> Check individual `*.csproj` files and `docker/docker-compose.yml` for
> exact package versions and external dependencies.

---

## Getting Started

### Prerequisites

- **.NET 10 SDK**
- **Docker Desktop** – for local backing services (MongoDB, RabbitMQ, KurrentDB)
- A recent IDE: Visual Studio 2022+, JetBrains Rider, or VS Code with C# Dev Kit

Optional (but recommended):

- `git` for source control
- **Bruno** (API client) to run the contracts in `BrewDocs/`
- A modern browser for Blazor debugging tools

---

## Running the Infrastructure (Docker)

The `docker` folder contains a `docker-compose.yml` and helper script.

From the repository root:

```powershell
cd docker
.\run-docker-compose.bat
```

This starts all required backing services:

| Service | Container | Host port(s) |
|---|---|---|
| KurrentDB (EventStoreDB) | `sales-eventstore` | `4113` (gRPC/HTTP) |
| MongoDB (sagas) | `sagas-mongodb` | `27017` |
| MongoDB (sales read model) | `sales-mongodb` | `37017` |
| RabbitMQ (+ stream plugin) | `rabbitmq` | `5672` AMQP · `5552` stream · `15672` management UI |

Inspect `docker/docker-compose.yml` for full configuration details.

---

## Running the Backend API (BrewApi)

The main API host is in `src/BrewApi/BrewUp.Rest`.

```powershell
cd src/BrewApi
dotnet run --project BrewUp.Rest/BrewUp.Rest.csproj
```

By default the API exposes:

- REST endpoints for all registered modules (MasterData, Sales, Purchases, Warehouse, Dashboards, Sagas, Chat/AI)
- Scalar OpenAPI UI for interactive exploration
- AI chat endpoint (`ChatModule`)

Configuration lives in `BrewUp.Rest/appsettings.json` (and `appsettings.Development.json`).
Key sections to configure before running:

```json
"BrewUp": {
  "MongoDbSettings": { "ConnectionString": "...", "DatabaseName": "BrewUp" },
  "EventStore":      { "ConnectionString": "esdb://localhost:4113?tls=false" },
  "RabbitMQ":        { "Host": "localhost", "Username": "guest", "Password": "guest", ... }
},
"AzureOpenAI": {
  "Endpoint": "...",
  "ApiKey": "...",
  "DeploymentName": "..."
}
```

---

## Running the MCP Server

The standalone Model Context Protocol server is in `src/BrewApi/Mcp/BrewUp.Mcp.McpServer`.

```powershell
cd src/BrewApi/Mcp/BrewUp.Mcp.McpServer
dotnet run
```

It exposes a stateless HTTP MCP endpoint at `/mcp` with the following tools:

| Tool | Description |
|---|---|
| `get_catalog_beers` | Returns active beers in the catalog |
| `get_open_sales_orders` | Returns currently open sales orders |
| `get_orders_by_customer` | Returns sales orders filtered by customer name |
| `get_late_sales_orders` | Returns late orders as of a given business date |

---

## Running the Web Front‑End (BrewSpa)

The Blazor app lives under `src/BrewSpa/BrewSpa`.

```powershell
cd src/BrewSpa/BrewSpa
dotnet run
```

Navigate to the URL printed in the console (commonly `https://localhost:xxxx`).

The SPA consumes BrewApi endpoints and is decomposed into feature modules:
Dashboards, MasterData, and Sales — each with their own `Application` and `Facade` layers.

---

## Running the Mobile App (BrewApp)

The .NET MAUI application is under `src/BrewApp/mobile/src/`.

1. Open the solution in Visual Studio 2022+ or JetBrains Rider.
2. Select the desired target (Android, iOS, Windows).
3. Build and run from the IDE.

The mobile app (`BrewApp.Mobile`) uses an `AppShell` navigation structure with
feature modules, services, components, and platform‑specific configurations.

---

## Tests

Test projects are distributed across the solution:

| Project | Scope |
|---|---|
| `BrewUp.MasterData.Tests` | MasterData domain & architecture |
| `BrewUp.Rest.Tests` | API integration & architecture |
| `BrewUp.Shared.Tests` | Shared utilities |
| `Sales/BrewUp.Sales.Tests` | Sales domain |
| `Purchases/BrewUp.Purchases.Tests` | Purchases domain |
| `Warehouse/BrewUp.Warehouse.Tests` | Warehouse domain |
| `Dashboards/BrewUp.Dashboards.Tests` | Dashboards |
| `Sagas/BrewUp.Sagas.Tests` | Sagas |
| `BrewSpa/Dashboards/BrewSpa.Dashboards.Tests` | Blazor dashboards |
| `BrewApp/mobile/tests/` | Mobile UI / integration |

Run all backend tests:

```powershell
cd src/BrewApi
dotnet test
```

---

## Architectural Notes

### DDD & Layers

- **Domain layer**
	- Encapsulates business rules, invariants, and ubiquitous language.
	- Uses aggregates, value objects, and domain events.
	- Pure C# code without infrastructure dependencies.
- **Application / Facade layer**
	- Implements use cases and orchestrates domain operations.
	- Coordinates between domain, read models, and infrastructure.
- **Infrastructure layer**
	- Persistence implementations, messaging, and external services.
	- Implementations of repositories, event stores, message publishers.

### Modular Monolith

- Each bounded context lives in its own folder tree and assemblies.
- Cross‑context communication goes through explicit contracts and
	messages (commands/events in `BrewUp.Shared/Messages/`) rather than shared
	tables or implicit coupling.
- Shared code is pushed either into
	- `BrewUp.Shared` for truly cross‑cutting concerns, or
	- `*.SharedKernel` for types shared inside a single context.
- The `BrewUp.Rest` composition root registers modules explicitly in
	`Program.cs`, making the feature surface immediately visible.

### Functional‑Style Modelling

While written in C#, many domain and application components are built
with a functional mindset:

- Emphasis on immutable value objects
- Clear input/output contracts for use cases
- Reduced side effects, with IO pushed to the infrastructure layer

### AI & MCP

- The `ChatModule` in `BrewUp.Rest` exposes AI chat endpoints backed by Azure OpenAI.
- `BrewUp.Mcp.McpServer` is a separate, independently deployable ASP.NET Core
	application that wraps ERP queries as MCP tools, enabling AI agents to
	interact with the ERP system via the Model Context Protocol.

---

## Roadmap & Status

This repository is under active development. Some planned or ongoing
areas include:

- Completing end‑to‑end flows for all bounded contexts
- Expanding test coverage (unit, integration, and contract tests)
- Hardening observability (distributed tracing, metrics dashboards)
- Improving documentation for each bounded context and module

Contributions in the form of issue reports, ideas, and pull requests
are welcome, but please treat this as a learning/reference project
rather than production‑ready software.

---

## License

See the `LICENSE` file at the root of the repository for the full
license text.

