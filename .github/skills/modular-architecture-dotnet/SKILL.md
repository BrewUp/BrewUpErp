---
name: modular-architecture-dotnet
description: Guidance for designing, implementing, and reviewing modular .NET systems with clear module boundaries, contracts, dependency flow, and vertical slices. Use when discussing modular monoliths or strongly bounded .NET architectures.
license: MIT
---

# Modular architecture for .NET skill

Use this skill when designing or modifying a .NET solution organized around modules, bounded contexts, vertical slices, or a modular monolith approach.

## Architectural intent

The primary goal is to keep each module autonomous enough that teams can change it with minimal ripple effects.

A module should:

- own a cohesive business capability
- encapsulate its domain rules and persistence details
- expose explicit contracts for other modules
- avoid leaking internal implementation details
- be testable independently

## Default mental model

Unless the repository shows a different pattern, think in terms of:

- **Module**: a business capability such as Catalog, Billing, Identity, Orders, Reporting
- **Inside a module**:
  - Domain
  - Application
  - Infrastructure
  - Presentation (Facade) with API endpoints if the module owns them
- **Between modules**:
  - contracts
  - events
  - use-case APIs
  - orchestration at the application boundary

## Dependency rules

Apply these rules strictly unless the task explicitly asks to relax them:

1. Domain code must not depend on infrastructure.
2. One module must not reach into another module's database tables, entity types, or internal services.
3. Cross-module calls must go through explicit contracts, integration events, or public application interfaces.
4. Shared code must be minimal, stable, and truly cross-cutting.
5. Avoid creating a large "shared" project that becomes a dumping ground.
6. Dependency direction should point toward abstractions and stable boundaries.

## Module boundaries

When analyzing or generating code, ask:

- Which module owns this use case?
- Which module owns the source of truth for this data?
- Is this behavior domain logic, application orchestration, or infrastructure?
- Does any proposed dependency violate module autonomy?

If ownership is unclear, prefer placing code with the module that owns the business rule, not the caller.

## Recommended project and folder patterns

A common shape is one of these:

### Option A: one project per module plus internal folders

- `Modules/Orders`
- `Modules/Billing`
- `Modules/Catalog`

Each module contains folders for Domain, Application, Infrastructure, and endpoints.

### Option B: multiple projects per module

- `Orders.Domain`
- `Orders.Application`
- `Orders.Infrastructure`
- `Orders.Contracts`

Use this when the codebase is large enough that physical separation adds real value.

Prefer the lightest structure that still preserves boundaries.

## Contracts and integration

When one module needs something from another:

- expose a contract, query interface, API, or integration event
- pass DTOs, messages, or dedicated response models, not internal entities
- keep contracts small and intention revealing
- version contracts deliberately if they are externally consumed

For asynchronous collaboration:

- favor domain or integration events for decoupled reactions
- make handlers idempotent when duplicate delivery is possible
- capture enough context in events to avoid fragile callback behavior

## Vertical slices inside modules

Inside a module, prefer organizing by use case when practical.

Example:

- `Orders/Application/CreateOrder`
- `Orders/Application/CancelOrder`
- `Orders/Application/GetOrderDetails`

Each slice can contain:

- command or query
- handler or service
- validator
- DTOs
- tests

This often keeps changes localized and avoids bloated service classes.

## Data ownership

- Each module owns its data and schema.
- Do not directly query another module's persistence store.
- For reads across modules, use dedicated read models, APIs, projections, or synchronization patterns.
- For reporting scenarios, consider separate projections rather than cross-module joins in core transactional code.

## Transactions and consistency

- Preserve transactional consistency within a module boundary.
- Across module boundaries, prefer eventual consistency unless there is a compelling reason otherwise.
- Be explicit about consistency expectations in workflows that span modules.
- Avoid distributed transaction assumptions unless the platform and architecture explicitly support them.

## API and presentation guidance

- Keep controllers or endpoints thin.
- Route requests to the owning module's application layer.
- Do not centralize all business logic in a global API layer.
- Keep request and response contracts aligned with the module's public behavior.

## Testing modular systems

Expect tests at multiple levels:

- unit tests for domain and application logic inside a module
- integration tests for module infrastructure and public endpoints
- architectural tests that verify dependency rules and forbidden references
- workflow tests for important cross-module scenarios

When possible, add tests that catch boundary violations early.

## Refactoring guidance

When moving toward a modular architecture:

1. Identify bounded contexts and ownership.
2. Stop new boundary violations first.
3. Extract contracts before moving implementations.
4. Move one use case at a time.
5. Add architectural tests to prevent regression.
6. Remove obsolete shared utilities after replacement paths exist.

## Review checklist

Flag these issues:

- one module reading or writing another module's tables directly
- domain entities leaking across modules
- infrastructure references from domain code
- large shared projects containing business logic from multiple modules
- controllers orchestrating several modules directly with no clear application service boundary
- circular project references
- cross-module synchronous chatty calls for simple workflows
- missing ownership for business rules

## Generation guidelines

When generating code or plans:

- name the owning module explicitly
- keep new files inside the owning module first
- define public contracts intentionally
- separate domain rules from orchestration and infrastructure
- note any events, interfaces, mappings, registrations, and tests needed
- call out tradeoffs if a simpler layered design may be more suitable than extra modular complexity

## Output style

- Explain changes in terms of ownership and dependency flow.
- Prefer concrete repository changes over abstract architecture talk.
- Keep the design strict enough to be enforceable in code review.
