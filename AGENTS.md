# BrewUpErp — Agent Guide

Sample ERP demonstrating DDD + modular monolith + CQRS + event-driven architecture on .NET 10.

## Architecture Essentials

- **Modular monolith**: each bounded context is its own folder tree and assembly set. Cross-context communication only through explicit messages (`BrewUp.Shared.Messages/`), never direct DB or service calls.
- **Bounded contexts**: `MasterData`, `Sales`, `Purchases`, `Warehouse`, `Dashboards`, `Sagas`, `Knowledge`, `Chat` (Mother). Each has `Domain/`, `Facade/`, `Infrastructure/`, `ReadModel/`, `SharedKernel/`, and `Tests/`. Some add `Entities/` or `McpServer/`.
- **Composition root**: `src/BrewApi/BrewUp.Rest/Module/` — each feature registers via `IModule`. Registration tree: `Add<Module>Facade()` → `AddDomain()` + `AddInfrastructure()` + `AddReadModel()`. Service lifetimes: domain/facade/persisters are `AddScoped`; `IMongoClient` is `AddSingleton`; `IPersister` is `AddKeyedScoped` per module.
- **CQRS framework**: Muflone 10.x (commands, events, sagas, EventStore + RabbitMQ transports). Custom lightweight mediator in `BrewUp.Mediator`. Functional utilities via Lena.
- **Two frontends**: Blazor WASM (`BrewSpa`) and React/Vite/TypeScript (`BrewReact`). React likely newer/in-progress.
- **MCP servers**: one per context (`BrewUp.*.McpServer`), independently deployable. SSE-based HTTP MCP endpoints.
- **Solution format**: `.slnx` (new .NET XML format, not classic `.sln`).
- **No centralized package management**: No `Directory.Build.props` or `Directory.Packages.props` — each `.csproj` pins its own versions.

## Infrastructure (Docker)

Two separate compose stacks. Backing services:

```bash
cd docker && docker compose up -d
```

| Service | Port | Purpose |
|---|---|---|
| KurrentDB (EventStoreDB fork) | 4113 | Event store (domain events + sagas) |
| MongoDB (sagas) | 27017 | Saga persistence |
| MongoDB (sales) | 37017 | Sales read model |
| RabbitMQ 4.x (+ stream plugin) | 5672, 5552, 15672 | Message bus |

The MCP servers + Knowledge agent have their own stack: `src/BrewApi/mcp-docker-compose.yml`
(identical copy at `infra/Docker/mcp-docker-compose.yml`). Build with
`--project-directory src/BrewApi` so relative `context: .` and `env_file: .env` resolve; a
`src/BrewApi/.env` (copy of `infra/Docker/env.example`) is required. Full build/run flow → `README.md`.

## Commands

```bash
# Backend API (default: http://localhost:6094, Scalar UI at /scalar)
dotnet run --project src/BrewApi/BrewUp.Rest/BrewUp.Rest.csproj

# All backend tests
dotnet test src/BrewApi

# Single test project
dotnet test src/BrewApi/Sales/BrewUp.Sales.Tests

# Architecture tests only (uses NetArchTest.Rules)
dotnet test src/BrewApi --filter "FullyQualifiedName~Architecture"

# React frontend (proxies /v1/* and /hubs/* to :6094)
cd src/BrewReact && npm run dev

# React tests / lint / build
npm run test:run   # vitest single run
npm run lint       # eslint . (flat config)
npm run build      # tsc -b && vite build

# MCP server example
dotnet run --project src/BrewApi/MasterData/BrewUp.MasterData.McpServer

# Full stack via Aspire AppHost (needs Parameters:* user secrets + pre-built MCP images; see README)
dotnet run --project src/BrewApi/BrewOrchestrator.Host/BrewOrchestrator.Host.csproj
```

## Conventions That Differ From Defaults

- **Naming**: `internal sealed` for infrastructure services/persisters and handlers in Infrastructure/ReadModel; domain command handlers are `public sealed` (extend `Muflone` `CommandHandlerAsync<T>`). Aggregates/entities have a `protected` parameterless ctor for EventStore/MongoDB deserialization. Domain IDs extend `Muflone.DomainId`, suffix `Id`.
- **Testing**: BDD-style via Muflone's `CommandSpecification<TCommand>`. Class names = scenario names (`CreateSalesOrderSuccessfully`).
- **DI helpers**: one `internal static` extension method per layer (e.g. `SalesFacadeHelper`, `SalesInfrastructureHelper`, `ReadModelHelper`), called from Facade only. No manual service registration in `Program.cs`.
- **NuGet**: `Muflone` 10.x (±`Muflone.SpecificationTests` 10.x), `Lena` 1.1, `NetArchTest.Rules` 1.3, xunit 2.9.
- **Architecture guard**: `NetArchTest.Rules` enforces: no cross-module deps, namespace compliance, Domain-isolated-from-Infrastructure.
- **API integration tests**: `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`).

## Instruction Files (`.github/`)

- `.github/AGENTS.md` — brief architecture principles (modular monolith, bounded context rules)
- `.github/bounded-contexts/` — detailed definitions per context (language, ownership, events, invariants)
- `.github/skills/` — **single source of truth** for skill files (arch-tests, C#, .NET, modular architecture, RabbitMQ, journal). Consumed natively by GitHub Copilot; opencode loads them via symlinks in `.opencode/skills/*` → `../../.github/skills/*` (no copies, same files).

Load a skill with `skill` tool when a task matches its description.

## Gotchas

- **`**/appsettings.Development.json` is git-ignored**: create it locally (copy the repo-root one if available) or the API runs on base `appsettings.json`, whose connection strings are placeholders.
- **Aspire AppHost exists** as `BrewOrchestrator.Host` (+ `BrewOrchestrator.ServiceDefaults`) but references MCP images **by name** — build them first (`src/BrewApi/mcp-docker-compose.yml`). Secrets live in AppHost user secrets under `Parameters:*`.
- **Mother A2A is disabled by default**: `BrewUp:Mother:A2A:Enabled` is `false` in `appsettings.json`.
- **MCP ports differ by launch mode**: containers use 8081–8084 (knowledge agent 8080); `dotnet run` uses launchSettings ports (agent 5005, knowledge MCP 5236, masterdata 5007, warehouse 5279, sales 5229). Point `BrewUp:McpServers:*` / `BrewUp:Mother:A2A:KnowledgeAgentUrl` at the right ones.
- **React lint is a fitness function**: `eslint-plugin-boundaries` errors on cross-feature imports (`src/features/*` may only import `src/shared/*`).
- **No CI/CD**: `.github/workflows/` is empty.
- **`.env` files**: React-only (`src/BrewReact/.env.development`, `.env.test`). Backend config is in `appsettings.json`; `src/BrewApi/.env` feeds the MCP containers only.
- **React path aliases**: `@features/*` → `src/features/*`, `@shared/*` → `src/shared/*`.
- **README.md is the canonical deep reference** (AI layer, Aspire orchestration, telemetry, Bicep deployment, ports); this file is the quick-start.
