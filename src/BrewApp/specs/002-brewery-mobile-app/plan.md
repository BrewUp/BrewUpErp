# Implementation Plan: Brewery Mobile App

**Branch**: `002-brewery-mobile-app` | **Date**: 2026-02-11 | **Spec**: [specs/002-brewery-mobile-app/spec.md](../002-brewery-mobile-app/spec.md)
**Input**: Feature specification from `/specs/002-brewery-mobile-app/spec.md`

## Summary

Build a .NET MAUI mobile application for brewery staff that provides a modern,
intuitive UI to navigate between Sales, Masterdata (beers catalog), Purchases,
Warehouse, and Dashboard contexts. All data (sales summaries, catalog, orders,
stock, dashboard metrics) is loaded and updated via external HTTP APIs exposed
by the existing BrewUp ERP backend. The app will centralize API configuration
in a dedicated configuration file so environments and endpoints can be managed
without code changes.

## Technical Context

**Language/Version**: .NET MAUI with C# targeting .NET 10 / MAUI 10
**Primary Dependencies**: .NET MAUI, .NET Generic Host/DI, HttpClient,
System.Text.Json (or equivalent JSON serializer), optional resilience library
for HTTP (e.g., Polly)
**Storage**: Online-first. External APIs are the system of record. The app
uses secure storage/preferences for authentication tokens and light caching
(e.g., last-viewed summaries, catalog snapshot). No local relational database
in v1.
**Testing**: xUnit (or NUnit) for unit tests of view models and services;
integration tests for API client contracts; UI/UX tests via .NET MAUI test
harness or cross-platform UI test framework (exact choice NEEDS
CLARIFICATION).
**Target Platform**: Android and iOS mobile devices; tablet-friendly layouts
for larger screens.
**Project Type**: mobile
**Performance Goals**: Initial app launch to first dashboard screen under 2
seconds on typical mid-range devices; navigation between contexts (e.g., from
Sales to Warehouse) under 500ms once data is cached; lists (catalog, orders,
stock) scroll smoothly at 60fps where possible.
**Constraints**: Online-first (no full offline order creation in v1);
communication only over HTTPS; all data read/written via external APIs (no
direct database access); API endpoints and credentials MUST be configured via a
dedicated configuration file; secrets MUST NOT be stored in plaintext.
**Scale/Scope**: Single brewery deployment initially; dozens of concurrent
internal users; data volume on the order of thousands of beers, tens of
thousands of orders, and daily sales summaries by period.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Maintainable, Modular Architecture**: The app will be organized by
	feature/context (Sales, Catalog, Orders, Warehouse, Dashboard, Shell
	navigation), with shared infrastructure modules for HTTP, configuration, and
	authentication. External APIs will be consumed through dedicated service
	interfaces so that UI layers and view models depend on abstractions rather
	than raw HttpClient calls.
- **II. Code Quality & Readability**: The mobile project will adopt the
	existing BrewApp coding standards (formatting, naming, DI patterns).
	ViewModels and services will follow consistent patterns (async methods,
	error-handling conventions), and public surfaces (navigation routes, API
	clients, configuration types) will be documented.
- **III. Testing Discipline (NON-NEGOTIABLE)**: New view models and API client
	services MUST include unit tests. Key flows (sales summary retrieval, order
	creation, catalog search, stock snapshot) MUST have integration or
	higher-level tests verifying behaviour against API contracts or suitable
	stubs. Bug fixes in these areas MUST add regression tests.
- **IV. Consistent User Experience**: Navigation will be implemented with a
	single MAUI Shell (or equivalent central navigation model) so all contexts
	share consistent patterns for tabs, headers, and back navigation. Shared UI
	components (cards, list items, filters) will be reused across contexts to
	keep the experience coherent.
- **V. Performance & Observability**: API access will be centralized in a
	reusable HTTP layer that can log requests, responses, and timings. The plan
	assumes server-side observability for most metrics; on the client we will at
	minimum log failures and slow calls and avoid blocking the UI thread during
	network operations.

At this stage there are no intentional constitution violations. Any future
exceptions (e.g., unavoidable cross-feature coupling) MUST be recorded in the
Complexity Tracking table with justification.

## Project Structure

### Documentation (this feature)

```text
specs/002-brewery-mobile-app/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── checklists/
		└── requirements.md  # Specification quality checklist
```

### Source Code (repository root)

```text
# Mobile application (new project)
mobile/
├── src/
│   ├── Configuration/
│   │   ├── ApiSettings.cs
│   │   └── apiSettings.json           # Environment-specific API configuration
│   ├── Features/
│   │   ├── Sales/
│   │   ├── Catalog/
│   │   ├── Orders/
│   │   ├── Warehouse/
│   │   └── Dashboard/
│   ├── Services/
│   │   ├── Api/                       # Typed API clients per context
│   │   └── Mapping/                   # DTO ↔ domain/view model mappers
│   ├── Components/                    # Shared UI components
│   └── App.xaml(.cs)                  # Shell and root application
└── tests/
		├── unit/
		└── ui/
```

**Structure Decision**: Use a feature-first mobile project under `mobile/`
with shared configuration and API service layers. Each business context
corresponds to a feature folder (Sales, Catalog, Orders, Warehouse,
Dashboard). API settings are centralized in `Configuration/apiSettings.json`
and bound to a strongly-typed `ApiSettings` class to keep environment-specific
details out of feature code.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|---------------------------------------|

