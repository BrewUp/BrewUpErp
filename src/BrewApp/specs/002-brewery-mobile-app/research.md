# Research: Brewery Mobile App (.NET MAUI + External APIs)

## Technology Stack Decisions

### .NET Runtime and UI Framework

- **Decision**: Use .NET 10 with .NET MAUI 10 and C# for the mobile app.
- **Rationale**: .NET 10 keeps the project on the current, modern .NET platform with strong MAUI support and access to the latest language/runtime features. It aligns with the rest of the BrewUp ERP ecosystem and positions the app for long-term support.
- **Alternatives considered**:
  - Older MAUI targets (e.g., .NET 8 / MAUI 8): Shorter support window relative to the chosen platform; no clear benefit for this new mobile feature.
  - Non-.NET stacks (Flutter, React Native): Would introduce additional technology stacks and expertise requirements, reducing maintainability for a .NET-focused team.

### HTTP and Serialization Stack

- **Decision**: Use `HttpClient` via DI with typed API client services per context, and `System.Text.Json` for JSON serialization.
- **Rationale**: `HttpClient` is the standard, well-supported option in .NET for HTTP calls and integrates cleanly with DI. `System.Text.Json` is performant, supported in-box, and avoids external serializer dependencies.
- **Alternatives considered**:
  - Refit or similar REST client libraries: Provide convenience but add another abstraction and dependency; can be introduced later if desired.
  - Newtonsoft.Json: Mature and flexible but adds an extra dependency where `System.Text.Json` is sufficient for these contracts.

### Testing Approach

- **Decision**: Use xUnit for unit tests of view models and services, plus integration tests around API clients and basic navigation flows; evaluate a .NET MAUI UI test harness for high-value end-to-end scenarios.
- **Rationale**: xUnit is widely used in .NET, integrates easily with CI, and keeps tests familiar for most .NET developers. Focusing UI tests only on critical flows keeps the suite maintainable.
- **Alternatives considered**:
  - NUnit or MSTest: Also viable but offer no strong advantage for this project; standardizing on xUnit reduces fragmentation.
  - Heavy UI automation for all screens: More coverage but higher flakiness and maintenance cost, not justified for the first iteration.

## Integration and Data Ownership

### Relationship to BrewUp ERP Backend

- **Decision**: Treat the mobile app as a companion UI client; BrewUp ERP backend remains the system of record for sales, catalog, orders, and warehouse data.
- **Rationale**: The user explicitly states that all data will be provided via external APIs. Keeping the backend as the system of record avoids duplicating business rules and reduces complexity around synchronization and conflict resolution.
- **Alternatives considered**:
  - Mobile-first system of record: Would require implementing full domain logic and persistence on-device or in a separate service, significantly expanding scope.
  - Hybrid ownership: Possible (e.g., mobile as primary for orders only), but adds complexity without clear benefit for this initial feature.

### Online vs Offline Behaviour

- **Decision**: Online-first with limited caching: the app requires connectivity for creating and updating orders, but may cache recent sales summaries, catalog data, and basic stock snapshots for faster reloads and limited read-only use when briefly offline.
- **Rationale**: This matches the external-API-first architecture while keeping implementation manageable. It improves perceived performance and resilience without designing a full offline sync system.
- **Alternatives considered**:
  - Strict online-only (no caching): Simplest to implement but leads to a poor experience in areas with spotty connectivity and slower perceived performance.
  - Full offline support (including order creation and conflict resolution): Strong for field work but requires significant additional design (local DB, sync engine, conflict strategies), which can be considered as a future feature.

### Roles and Permissions

- **Decision**: Start with four base roles managed by the backend: Owner/Manager, Sales Rep, Warehouse Staff, and Admin, with coarse-grained permissions per context enforced server-side and reflected in the mobile UI (e.g., hiding or disabling actions).
- **Rationale**: This maps well to typical brewery operations and keeps the mobile client relatively simple (it primarily honours backend-issued claims/permissions). Fine-grained rules remain centralized in the ERP.
- **Alternatives considered**:
  - Single "Staff" role: Easiest but offers no separation of duties and risks overexposure of sensitive information.
  - Fully client-driven, fine-grained permissions: More flexible but duplicates logic that is better maintained on the server.

## Configuration Strategy

### API Settings Configuration File

- **Decision**: Use a JSON configuration file (e.g., `apiSettings.json`) under `mobile/src/Configuration/` bound to a strongly typed `ApiSettings` class. This file holds per-environment base URLs and general settings for the external APIs.
- **Rationale**: Centralizing API configuration in a file avoids hardcoding URLs and allows environment changes (dev/test/prod) without recompilation. Binding to a typed class keeps usage discoverable and safe for new developers.
- **Alternatives considered**:
  - Hardcoding API URLs in services: Quicker initially but brittle and harder to manage across environments.
  - Using platform-specific config only (e.g., Android/iOS native config): Possible, but less consistent across platforms than using a shared .NET configuration mechanism.

## Open Questions to Confirm Later (Non-Blocking for Planning)

- Exact .NET and MAUI versions to align with the broader BrewUp solution (current plan assumes .NET 8 / MAUI 8).
- Final choice of UI test framework for MAUI (to be validated once the main views are in place).
