# BrewUpErp

BrewUpErp is an end‑to‑end sample ERP built to demonstrate

- Domain‑Driven Design (DDD)
- Modular / bounded‑context architecture
- Functional‑style domain modelling
- Clean, testable application layers

The solution is intentionally realistic but still small enough to study.
It consists of:

- **BrewApi** – backend HTTP API
- **BrewSpa** – Blazor web front‑end
- **BrewApp** – .NET MAUI mobile application

> Note: this repository is a work in progress. Expect breaking changes
> and incomplete features, especially in less‑used flows.

---

## Solution Structure

High‑level folder layout:

- `src/BrewApi` – backend API and domain implementation
	- `BrewUp.Rest` – ASP.NET Core Web API host
	- `BrewUp.Shared` – shared abstractions, value objects, helpers
	- `MasterData/*` – Master Data bounded context (domain, facade, read model, infrastructure, shared kernel)
	- `Sales/*` – Sales bounded context (domain, facade, read model, infrastructure, shared kernel, tests)
	- `BrewUp.Infrastructure` – cross‑cutting infrastructure (MongoDB, RabbitMQ, logging, etc.)
	- `BrewUp.*.Tests` – backend/unit/architecture tests
- `BrewSpa` – Blazor front‑end
	- `BrewSpa` – main Blazor app (layouts, pages, wwwroot)
	- `BrewSpa.Shared` – shared client models and helpers
	- `BrewSpa.Shared.Components`, `Dashboards`, `MasterData`, `Sales` – modularized UI feature areas
- `BrewApp` – .NET MAUI mobile solution
	- `src/*` – MAUI shell, shared UI/core libraries, feature modules
	- `tests/*` – mobile/UI tests
- `BrewErp` – Bruno API contracts & examples (e.g. `GetCustomers.bru`)
- `docker` – `docker-compose.yml` and helper script for local infrastructure
- `Docs` – sample API payloads and documentation fragments

### Bounded Contexts & Modules

The ERP is decomposed into modules that map to bounded contexts:

- **MasterData** – core catalog and reference data (e.g., customers, products)
- **Sales** – sales orders and related workflows
- (Additional contexts like Purchase/Warehouse are being modelled incrementally.)

Each context follows a similar layout:

- `*.Domain` – Aggregates, value objects, domain events, domain services
- `*.SharedKernel` – types shared only inside the bounded context
- `*.Infrastructure` – persistence and integration (MongoDB, messaging, etc.)
- `*.ReadModel` – denormalized read models and projections
- `*.Facade` – application services / use‑case orchestration

This structure keeps the domain model independent from infrastructure and UI,
and encourages explicit module boundaries.

---

## Technology Stack

- **Runtime**: .NET (currently targeting `net10.0` for the API)
- **API**: ASP.NET Core Web API
	- OpenAPI/Swagger via Swashbuckle and Scalar
	- Observability with OpenTelemetry and Azure Monitor exporter
	- Logging with Serilog (console + file)
- **Architecture**:
	- DDD with explicit domain and application layers
	- Modular monolith style, grouped by bounded context
	- CQRS and message‑driven patterns inspired by **Muflone**
- **Persistence & Messaging**:
	- MongoDB (document storage)
	- RabbitMQ (message bus)
- **Web UI**: Blazor (server or WebAssembly, depending on configuration)
- **Mobile**: .NET MAUI (Android / iOS / Windows)

> Check individual `*.csproj` files and `docker/docker-compose.yml` for
> exact framework versions and external dependencies.

---

## Getting Started

### Prerequisites

- .NET SDK compatible with the target frameworks
	- For the backend: `.NET 10` (as `net10.0` is targeted)
- Docker Desktop (for local MongoDB, RabbitMQ, and other backing services)
- A recent IDE: Visual Studio, Rider, or VS Code with C# support

Optional (but recommended):

- `git` for source control
- A modern browser for Blazor debugging tools

---

## Running the Infrastructure (Docker)

The `docker` folder contains a `docker-compose.yml` and helper script.

From the repository root:

```bash
cd docker
./run-docker-compose.bat   # on Windows PowerShell / CMD
```

This typically starts the required supporting services (MongoDB,
RabbitMQ, etc.). Inspect `docker-compose.yml` for details and port
configuration.

---

## Running the Backend API (BrewApi)

The main API host is in `src/BrewApi/BrewUp.Rest`.

From the repository root:

```bash
cd src/BrewApi
dotnet run --project BrewUp.Rest/BrewUp.Rest.csproj
```

By default the API will expose:

- HTTP endpoints for each bounded context (e.g., MasterData, Sales)
- OpenAPI/Swagger UI (and/or Scalar UI) for interactive exploration

Configuration is managed via `appsettings.json` and environment‑specific
files such as `appsettings.Development.json` inside `BrewUp.Rest`.

---

## Running the Web Front‑End (BrewSpa)

The Blazor web app lives under `BrewSpa/BrewSpa`.

From the repository root:

```bash
cd BrewSpa/BrewSpa
dotnet run
```

Then navigate to the URL printed in the console (commonly
`https://localhost:xxxx`).

The SPA consumes the BrewApi endpoints and is structured into
feature‑specific modules (Dashboards, MasterData, Sales, etc.), plus
shared components and models.

---

## Running the Mobile App (BrewApp)

The .NET MAUI application is under `BrewApp`.

Typical workflow:

1. Open `BrewApp/BrewUpErp.Mobile.slnx` in Visual Studio or Rider.
2. Select the desired target (Android, iOS, Windows).
3. Build and run from the IDE.

The mobile app uses a shell navigation structure and references
feature modules (MasterData, Sales, Warehouse, etc.) and shared
libraries (`BrewApp.Shared.Core`, `BrewApp.Shared.UI`).

---

## Tests

The solution includes several test projects under `src/BrewApi` and
`BrewApp/tests`.

Examples:

- `src/BrewApi/BrewUp.MasterData.Tests`
- `src/BrewApi/BrewUp.Rest.Tests`
- `src/BrewApi/Sales/BrewUp.Sales.Tests`

Run all backend tests from the repository root with:

```bash
cd src/BrewApi
dotnet test
```

Additional mobile/UI tests can be run from the appropriate `tests`
projects under `BrewApp` using your IDE or `dotnet test`.

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
	messages rather than shared tables or implicit coupling.
- Shared code is pushed either into
	- `BrewUp.Shared` for truly cross‑cutting concerns, or
	- `*.SharedKernel` for types shared inside a single context.

### Functional‑Style Modelling

While written in C#, many domain and application components are built
with a functional mindset:

- Emphasis on immutable value objects
- Clear input/output contracts for use cases
- Reduced side effects, with IO pushed to the infrastructure layer

---

## Roadmap & Status

This repository is under active development. Some planned or ongoing
areas include:

- Completing end‑to‑end flows for all modules (not only Sales/MasterData)
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

