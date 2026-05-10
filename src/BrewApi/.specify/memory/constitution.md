# BrewUp ERP API — Constitution

> **Version**: 1.0.0 — Ratified: 2026-05-10  
> **Status**: Active  
> **Scope**: All modules of the BrewUp ERP REST API (`BrewApi` solution)

This constitution is the **architectural DNA** of the BrewUp ERP solution.
Every specification, plan, task list, and implementation must comply with it.
AI coding agents must load and enforce this document before generating any artifact.

---

## Preamble

BrewUp is a modular-monolith ERP for the craft-brewery domain.
Its REST API is hosted in `BrewUp.Rest` and is composed of independently evolvable modules: **MasterData**, **Sales**, **Warehouse**, **Sagas**, and **Dashboards**.

The system is built on:

| Concern | Technology |
|---------|------------|
| Web framework | ASP.NET Core minimal API (no controllers) |
| CQRS / DDD framework | Muflone |
| Write store | EventStoreDB (event-sourcing via `IRepository`) |
| Read store | MongoDB (projections via `IPersister` / `IReadModelPersister`) |
| Messaging | RabbitMQ (integration events via `IServiceBus`) |
| Result type | Lena (`Result<T>`, railway-oriented programming) |
| Validation | ASP.NET `IEndpointFilter` + `ValidationFilter<T>` |
| Tests | xUnit + Muflone.SpecificationTests + NetArchTest.Rules |

---

## Article I - Core Principles

### I. Test-First Development - E2E (NON-NEGOTIABLE)

Development MUST adopt a Test-First approach based on End-to-End (E2E) tests:

1. **Red (Pre-implementation)**: Write E2E tests that describe the expected behavior
   and verify that they FAIL before writing any production code.
2. **Green (Post-Implementation)**: Write the minimum code required to pass the E2E tests.
3. **Verify (Regression)**: All pre-existing E2E tests MUST continue to pass
   after every change.

**Testing Tool**: E2E tests are run using Playwright, available via
MCP (Model Context Protocol). Follow the operational guidelines in
`.specify/memory/playwright-typescript.instructions.md`.

**Mandatory Rules:**
- NO production code may be written without failing E2E tests that verify
  their existence
- E2E tests MUST be written and verified as failing BEFORE deployment
- E2E tests MUST be run BEFORE deployment to establish the regression
  baseline
- E2E tests MUST be run AFTER deployment to verify:
  - New requirements are met
  - No regressions have been introduced relative to the baseline
- Every commit MUST include or update the E2E tests related to the changes
- Tests MUST be independent, repeatable, and descriptive of the expected behavior

**Rationale:** E2E tests verify the actual behavior of the application from the
user’s perspective, ensuring that requirements are met and that no regressions
are introduced during iterative development.

### II. Simple Design (NON-NEGOTIABLE)

The software design MUST adhere to Kent Beck’s four rules of Simple Design,
applied in this specific order of priority:

1. **Passes the Tests (It Works)**: The code MUST pass all E2E and unit tests.
   No other design consideration takes precedence over functional correctness.

2. **Reveals Intention**: The code MUST clearly communicate its purpose. Names,
   structure, and abstractions MUST reveal the “why” and the “what,” not just the “how.”

3. **No Duplication (DRY)**: Every piece of knowledge MUST have a single, unambiguous, and
   authoritative representation in the system. Avoid all forms of duplication: logical,
   structural, or data-related.

4. **Fewest Elements**: The code MUST contain as few classes,
   methods, functions, and abstractions as possible. Do not add complexity that is not required by
   existing tests.

**Mandatory Rules:**
- The order of the four rules is MANDATORY: Rule 1 MUST be satisfied before
  Rule 2, Rule 2 before Rule 3, and Rule 3 before Rule 4
- Do not add abstractions, classes, or functions “just in case” (YAGNI — You Aren't Gonna
  Need It)
- Every element of complexity MUST be justified by a test that requires
  its existence

**Rationale:** The four rules of Simple Design guide us toward minimal and
correct solutions, avoiding over-engineering and keeping the code adaptable over time.

## Article II - Clean Code

The code MUST adhere to the principles of Clean Code to ensure readability,
maintainability, and quality over time.

**General Rules:**
- Follow the standard conventions of the language used
- Code MUST be self-documenting; comments explain the “why,” not the “what”
- Avoid duplicate code (DRY - Don't Repeat Yourself)
- Apply the KISS principle (Keep It Simple, Stupid)

## Article III — Modular Monolith Structure

### III.1 Module layout

Every bounded context is a **module** with exactly these .NET projects:

```
BrewUp.<Module>.Domain           # aggregates, command handlers, domain services
BrewUp.<Module>.Entities         # entity definitions (aggregate roots, value objects)
BrewUp.<Module>.Facade           # endpoints, facade interfaces, DI composition root
BrewUp.<Module>.Infrastructure   # EventStore/MongoDB adapters, persisters
BrewUp.<Module>.ReadModel        # projections, queries, read-model persisters
BrewUp.<Module>.SharedKernel     # commands, domain events, integration events, types
BrewUp.<Module>.Tests            # all tests for the module
```

Deviations require a documented ADR in the feature's `plan.md`.

### III.2 Module boundaries

- A module **must not** reference another module's `Domain`, `Entities`, `Infrastructure`, or `ReadModel` projects.
- Cross-module data flows **exclusively** through integration events published to `BrewUp.Shared.Messages.Events` and consumed by ACL handlers (`Acl/` inside `Facade`).
- Read-only cross-module lookups **may** use `BrewUp.Shared.ReadModel` DTOs surfaced through a public query interface.

### III.3 Shared kernel boundaries

`BrewUp.Shared` is the **only** genuinely shared project. It contains:

- `ExternalContracts/` — inbound/outbound JSON DTOs (one sub-folder per module)
- `Messages/Commands/` and `Messages/Events/` — integration event contracts
- `ReadModel/` — `IPersister`, `IReadModelPersister`, `IQueries`, `PagedResult<T>`
- `CustomTypes/`, `DomainIds/`, `Validators/`, `Helpers/`

`BrewUp.Shared` **must not** import any module project.

---

## Article IV — Naming Conventions

| Element | Rule | Example |
|---------|------|---------|
| Classes / interfaces / records | PascalCase | `CustomerDomainService` |
| Methods | PascalCase, verb+noun | `CreateCustomerAsync` |
| Private fields | `_camelCase` | `_salesOrderNumber` |
| Local variables / params | camelCase | `customerId` |
| Domain IDs | `<Entity>Id` extends `DomainId` | `CustomerId`, `SalesOrderId` |
| Value objects | Descriptive noun | `RagioneSociale`, `PartitaIva` |
| Commands | Imperative verb phrase | `CreateSalesOrder` |
| Domain events | Past tense | `SalesOrderCreated` |
| Integration events | Past tense, in `BrewUp.Shared.Messages.Events` | `CustomerCreated` |
| JSON DTOs | `<Action><Entity>Json` (inbound) / `<Entity>Json` (outbound) | `CreateCustomerJson`, `CustomerJson` |
| Read model DTOs | Entity name only, in `Dtos/` | `Customer`, `SalesOrder` |
| Facade DI helpers | `Add<Module>Facade()` | `AddMasterDataFacade()` |
| Endpoint classes | `<Entity>Endpoints` or `<Entity>Endpoint` | `CustomersEndpoint` |
| Test classes | BDD scenario name | `CreateSalesOrderSuccessfully` |

### IV.1 Visibility

| Visibility | Applies to |
|------------|-----------|
| `internal sealed` | domain services, command handlers, event handlers, facade implementations, query implementations, persisters |
| `public` | DI extension methods, endpoint mapping methods, interfaces, ExternalContract DTOs, SharedKernel messages |
| `protected` | parameterless constructor on every aggregate and DTO (required for EventStore / MongoDB deserialization) |
| `private` | aggregate state fields, internal factory helpers |

---

## Article V — Dependency Injection

### V.1 Registration hierarchy

Each module exposes a single public DI entry point on `IServiceCollection`:

```
Add<Module>Facade()
  └─ Add<Module>Domain()
  └─ Add<Module>Infrastructure()
  └─ Add<Module>ReadModel()
```

Internal helpers (`AddDomain`, `AddInfrastructure`, `AddReadModel`) are `internal static`.

### V.2 Service lifetimes

| Lifetime | Used for |
|----------|---------|
| `AddScoped` | domain services, facades, query services, queries, persisters, ACL event handlers |
| `AddSingleton` | `IMongoClient` (one connection pool per application) |
| `AddKeyedScoped` | `IPersister` — each module registers its own key to prevent cross-module collision |

### V.3 Keyed services pattern

```csharp
// Infrastructure helper
services.AddKeyedScoped<IPersister, MasterDataPersister>("masterdata");

// Resolved via constructor injection
internal sealed class CustomerDomainService(
    [FromKeyedServices("masterdata")] IPersister persister, ...)
```

### V.4 Module registration

Each feature module implements `IModule` and is registered in `BrewUp.Rest/Program.cs` via `RegisterModules()`.

---

## Article VI — API Design

### VI.1 Endpoint conventions

- Use **minimal API endpoint groups** (`app.MapGroup(...)`) — no MVC controllers.
- URL pattern: `/v{version}/{module}/{resource}` e.g. `/v1/masterdata/customers`.
- Every endpoint must declare `Produces(...)`, `WithSummary(...)`, `WithDescription(...)`, and `WithName(...)`.
- Validation endpoints use `.AddEndpointFilter<ValidationFilter<T>>()`.
- Group endpoints by entity in a single `internal static` class: `<Entity>Endpoint.cs`.

### VI.2 HTTP semantics

| Operation | Method | Success code |
|-----------|--------|-------------|
| Create resource | POST | 201 Created + Location header |
| Replace resource | PUT | 202 Accepted |
| Partial update | PATCH | 202 Accepted |
| Delete resource | DELETE | 202 Accepted |
| Fetch collection | GET | 200 OK + `PagedResult<T>` |
| Fetch single | GET | 200 OK + entity JSON |

### VI.3 Error responses

- Use ASP.NET `ProblemDetails` (`application/problem+json`).
- Include a UTC `timestamp` extension field on every problem response.
- Validation failures return 400 with error count in `detail`.
- Unhandled errors return 500.

### VI.4 OpenAPI

- Maintain `openapi.yaml` at repository root as the **single source of truth** for the API contract.
- Every new endpoint must be reflected in `openapi.yaml` before implementation begins.
- Use `$ref` for all reusable schemas, parameters, and responses.

---

## Article VII — Domain Model

### VII.1 Command handling

- Command handlers extend Muflone's `ICommandHandlerAsync<TCommand>`.
- Registration: `services.AddCommandHandler<THandler>()`.
- A command handler **must not** contain domain logic — it delegates to a domain service.

### VII.2 Domain services

- Implement `internal sealed class <Entity>DomainService`.
- Use Railway-Oriented Programming via Lena's `Result<T>` and `.BindAsync()` chain.
- Raise integration events via `IIntegrationEventPublisher` after state changes.

### VII.3 Aggregates / Entities

- Aggregates have a **static factory method** (`Create(...)`) that returns the instance after recording a domain event.
- Aggregates expose mutation methods that modify private state fields.
- Every aggregate has a `protected` parameterless constructor.

### VII.4 Domain events vs integration events

| Type | Location | Purpose |
|------|----------|---------|
| Domain event | `BrewUp.<Module>.SharedKernel/Messages/` | Internal state change within the module |
| Integration event | `BrewUp.Shared/Messages/Events/<Module>/` | Cross-module notification |

---

## Article VIII — Read Model

### VIII.1 Projections

- Projections are registered as `IDomainEventHandler<TEvent>` or `IIntegrationEventHandler<TEvent>`.
- They write to MongoDB using `IReadModelPersister`.
- They **must not** contain business logic — only DTO mapping and persistence.

### VIII.2 Queries

- Implement `IQueries` (or the typed query interface).
- Return `PagedResult<T>` for collections.
- They are `internal sealed` and registered via `AddReadModel()`.

---

## Article IX — Testing Requirements

### IX.1 Domain tests (mandatory)

Every command handler must have at least one `CommandSpecification<TCommand>` test using the Given-When-Expect pattern:

```csharp
public sealed class <Scenario> : CommandSpecification<TCommand>
{
    protected override IEnumerable<DomainEvent> Given() { ... }
    protected override TCommand When() => new(...);
    protected override ICommandHandlerAsync<TCommand> OnHandler() => new ...();
    protected override IEnumerable<DomainEvent> Expect() { yield return new ...; }
}
```

### IX.2 Architecture tests (mandatory)

Every module must have an `ArchitectureTests.cs` asserting:

1. No dependency on sibling modules' `Domain`, `Facade`, `Infrastructure`, or `ReadModel`.
2. All namespaces start with `BrewUp.<ModuleName>`.

See `.github/skills/arch-tests/SKILL.md` for the scaffold template.

### IX.3 Serialization tests (mandatory for integration events)

Every integration event must have a round-trip serialization test using Muflone's `Serializer`.

### IX.4 REST integration tests

The `BrewUp.Rest.Tests` project hosts integration tests against a running `WebApplicationFactory`. Tests must use `appsettings.Test.json` for configuration.

### IX.5 No production mocks

- Prefer real databases (MongoDB, EventStoreDB test containers) over in-memory mocks.
- Use `Testcontainers` when integration tests require infrastructure.

---

## Article X — Simplicity Gate

Before any implementation plan is accepted, apply this gate:

- [ ] Does the solution add the **minimum** necessary projects? (no extra abstraction layers)
- [ ] Are shared utilities truly cross-cutting or specific to one module?
- [ ] Is there any speculative "we might need this later" code? (reject it)
- [ ] Does the plan reuse existing Muflone, Lena, or ASP.NET features rather than wrapping them?

Violations must be documented and approved before proceeding.

---

## Article XI — Security Baseline

- All endpoints that mutate state must validate the request body via `ValidationFilter<T>`.
- Never log secrets, tokens, or PII.
- Use `CancellationToken` propagation in all async paths.
- Apply `cancellationToken.ThrowIfCancellationRequested()` at the start of every public async method.
- Validate all external input at the API boundary; do not re-validate inside domain services.
- Return `ProblemDetails` for all errors; never expose stack traces or internal exception messages.

---

## Article XII — Amendment Process

Modifications to this constitution require:

1. A written ADR (`adr-<NNN>-<slug>.md`) in the relevant spec folder.
2. Explicit documentation of the rationale and backwards-compatibility impact.
3. Review and approval noted in the ADR before any implementation proceeds.

---

## Quick-reference checklist (AI agents: verify before generating a plan)

- [ ] Module folder structure matches Article I.1
- [ ] No cross-module internal dependencies (Article I.2)
- [ ] Naming follows Article II
- [ ] DI hierarchy follows Article III
- [ ] Endpoints follow Article IV
- [ ] Domain model follows Article V
- [ ] Tests cover Articles VII.1–VII.4
- [ ] Simplicity gate passed (Article VIII)
- [ ] Security baseline applied (Article IX)
