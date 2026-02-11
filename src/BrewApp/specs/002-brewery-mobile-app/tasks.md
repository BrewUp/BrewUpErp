---

description: "Implementation tasks for Brewery Mobile App (.NET MAUI, external APIs)"
---

# Tasks: Brewery Mobile App

**Input**: Design documents from `/specs/002-brewery-mobile-app/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are required by the project constitution but are planned inline with implementation tasks rather than as a separate section.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions (planned structure from plan.md)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, base structure, and configuration for external APIs.

- [X] T001 Create `mobile/` solution folder and add a .NET MAUI 10 app project at `mobile/src/` aligned with plan.md structure
- [X] T002 Configure .NET MAUI Shell navigation scaffold in `mobile/src/App.xaml` and `mobile/src/App.xaml.cs`
- [X] T003 [P] Add configuration infrastructure and `ApiSettings` class in `mobile/src/Configuration/ApiSettings.cs` bound to `ApiSettings` JSON section
- [X] T004 [P] Add initial `mobile/src/Configuration/apiSettings.json` based on `specs/002-brewery-mobile-app/contracts/api-settings.example.json` (use dev/test endpoints)
- [X] T005 [P] Set up dependency injection container in `mobile/src/MauiProgram.cs` to register configuration, HttpClient, and feature services

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story can be fully implemented.

**⚠️ CRITICAL**: No user story work can complete until this phase is done.

- [X] T006 Implement shared HTTP client factory and base API client in `mobile/src/Services/Api/HttpClientFactory.cs` and `mobile/src/Services/Api/BaseApiClient.cs`
- [X] T007 [P] Implement authentication token storage and provider in `mobile/src/Services/Auth/AuthService.cs` (stubbed to work with existing backend auth)
- [X] T008 [P] Add basic error handling and toast/dialog service in `mobile/src/Services/Ui/NotificationService.cs` for API errors and empty states
- [X] T009 Define shared mapping helpers for API DTOs ↔ view models in `mobile/src/Services/Mapping/` (per entities from `data-model.md`)
- [X] T010 Create shared UI components for list items and cards in `mobile/src/Components/` (e.g., `BeerListItem`, `OrderListItem`, `SummaryCard`)
- [X] T011 Wire `ApiSettings` and HTTP clients into DI in `mobile/src/MauiProgram.cs` and verify they can be resolved in sample code

**Checkpoint**: Foundation ready – API configuration, HTTP layer, mappings, and basic navigation scaffold in place.

---

## Phase 3: User Story 5 - Navigation Across Contexts (Priority: P1) 🎯 MVP Shell

**Goal**: Provide a modern, intuitive navigation model letting users move between Sales, Masterdata/Catalog, Purchases, Warehouse, and Dashboard.

**Independent Test**: A first-time user can open the app and navigate between all main contexts via the Shell UI in a few taps.

### Implementation for User Story 5

- [X] T012 [US5] Define MAUI Shell routes and tab structure in `mobile/src/AppShell.xaml` (Sales, Catalog, Purchases, Warehouse, Dashboard)
- [X] T013 [P] [US5] Create placeholder pages and view models for each context in `mobile/src/Features/Sales/`, `Catalog/`, `Orders/`, `Warehouse/`, `Dashboard/`
- [X] T014 [US5] Implement consistent headers, icons, and navigation patterns across all context pages using shared components in `mobile/src/Components/`
- [X] T015 [US5] Add basic empty-state handling for each context page when no data is available yet (e.g., "No sales for today") using `NotificationService`

**Checkpoint**: Shell navigation and basic pages exist for each context; app is navigable even before data is wired up.

---

## Phase 4: User Story 1 - Review Sales Overview (Priority: P1)

**Goal**: Let users quickly see key sales summaries for configurable periods.

**Independent Test**: A user can open the Sales context, choose a period, and see an updated sales summary panel.

### Implementation for User Story 1

- [X] T016 [P] [US1] Implement Sales API client in `mobile/src/Services/Api/SalesApiClient.cs` using `/sales/summary` from `contracts/mobile-app-apis.openapi.yaml`
- [X] T017 [P] [US1] Implement `SalesSummaryViewModel` in `mobile/src/Features/Sales/SalesSummaryViewModel.cs` (period selection, load state, error handling)
- [X] T018 [US1] Implement Sales dashboard page UI in `mobile/src/Features/Sales/SalesPage.xaml` and code-behind to bind to `SalesSummaryViewModel`
- [X] T019 [US1] Integrate mappings between API `SalesSummary` DTO and view model in `mobile/src/Services/Mapping/SalesMappingProfile.cs`
- [X] T020 [US1] Add unit tests for `SalesSummaryViewModel` behaviour (period changes, success/error paths) in `mobile/tests/unit/Features/Sales/SalesSummaryViewModelTests.cs`

**Checkpoint**: Sales summary is fully functional and independently testable.

---

## Phase 5: User Story 2 - Manage Beers Catalog (Priority: P1)

**Goal**: Allow users to browse and search the beers catalog by key attributes.

**Independent Test**: A user can open the Catalog context, search by name or style, and see filtered beer results.

### Implementation for User Story 2

- [X] T021 [P] [US2] Implement Catalog API client in `mobile/src/Services/Api/CatalogApiClient.cs` using `/catalog/beers`
- [X] T022 [P] [US2] Implement `CatalogViewModel` in `mobile/src/Features/Catalog/CatalogViewModel.cs` with search and filter state
- [X] T023 [US2] Implement Catalog page UI in `mobile/src/Features/Catalog/CatalogPage.xaml` bound to `CatalogViewModel` and shared `BeerListItem` component
- [X] T024 [US2] Implement mapping between API `Beer` DTO and internal `Beer` view model in `mobile/src/Services/Mapping/CatalogMappingProfile.cs`
- [X] T025 [US2] Add unit tests for `CatalogViewModel` search and filter logic in `mobile/tests/unit/Features/Catalog/CatalogViewModelTests.cs`

**Checkpoint**: Beers catalog browsing and searching works independently of other stories.

---

## Phase 6: User Story 3 - Create and Track Sales Orders (Priority: P1)

**Goal**: Allow creation of new sales orders and viewing of current and past orders.

**Independent Test**: A user can create an order from the app and later find it in current/past orders with correct details.

### Implementation for User Story 3

- [ ] T026 [P] [US3] Implement Orders API client in `mobile/src/Services/Api/OrdersApiClient.cs` using `/orders` and `/orders/{id}`
- [ ] T027 [P] [US3] Implement `OrdersListViewModel` in `mobile/src/Features/Orders/OrdersListViewModel.cs` to load and filter current/past orders
- [ ] T028 [P] [US3] Implement `OrderDetailViewModel` in `mobile/src/Features/Orders/OrderDetailViewModel.cs` to build and validate new orders
- [ ] T029 [US3] Implement Orders list page in `mobile/src/Features/Orders/OrdersPage.xaml` and order detail page in `OrdersDetailPage.xaml` bound to respective view models
- [ ] T030 [US3] Implement mappings between API `Order`/`OrderLineItem` DTOs and internal models in `mobile/src/Services/Mapping/OrdersMappingProfile.cs`
- [ ] T031 [US3] Add unit tests for `OrdersListViewModel` and `OrderDetailViewModel` (creation, validation, happy/error paths) in `mobile/tests/unit/Features/Orders/`

**Checkpoint**: Order creation and tracking flows are fully functional and testable.

---

## Phase 7: User Story 4 - Warehouse & Stock Snapshot (Priority: P2)

**Goal**: Provide visibility into current stock levels per beer and location.

**Independent Test**: A user can open the Warehouse context and see stock levels for beers, optionally filtered by location.

### Implementation for User Story 4

- [ ] T032 [P] [US4] Implement Warehouse API client in `mobile/src/Services/Api/WarehouseApiClient.cs` using `/warehouse/stock`
- [ ] T033 [P] [US4] Implement `WarehouseViewModel` in `mobile/src/Features/Warehouse/WarehouseViewModel.cs` with grouping and filtering options
- [ ] T034 [US4] Implement Warehouse page UI in `mobile/src/Features/Warehouse/WarehousePage.xaml` bound to `WarehouseViewModel`
- [ ] T035 [US4] Implement mappings between API `StockItem` DTO and internal model in `mobile/src/Services/Mapping/WarehouseMappingProfile.cs`
- [ ] T036 [US4] Add unit tests for `WarehouseViewModel` (loading, grouping, filter behaviour) in `mobile/tests/unit/Features/Warehouse/WarehouseViewModelTests.cs`

**Checkpoint**: Warehouse snapshot is functional; feature can be tested independently of other stories.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements and hardening that affect multiple user stories.

- [ ] T037 [P] Add loading, empty, and error states consistently across all feature pages using `NotificationService` in `mobile/src/Services/Ui/`
- [ ] T038 [P] Add additional unit tests for shared mapping and API client behaviour in `mobile/tests/unit/Services/`
- [ ] T039 Implement basic performance checks and lightweight profiling for critical flows (Sales, Orders) and document findings in `specs/002-brewery-mobile-app/research.md`
- [X] T040 [P] Update or create README for the mobile project at `mobile/README.md` describing architecture, configuration, and how to run tests
- [ ] T041 Run through quickstart steps in `specs/002-brewery-mobile-app/quickstart.md` and confirm they are accurate; adjust where necessary

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies – can start immediately.
- **Foundational (Phase 2)**: Depends on Setup; blocks all user stories.
- **User Story Phases (3–7)**:
  - Depend on completion of Setup and Foundational phases.
  - **US5 Navigation** (Phase 3) should be implemented before or alongside other stories so screens exist and are navigable.
  - **US1 Sales**, **US2 Catalog**, **US3 Orders**, **US4 Warehouse** can proceed in parallel once shared infrastructure and navigation shell are in place.
- **Polish (Phase 8)**: Depends on the desired user stories being complete.

### User Story Dependencies

- **User Story 5 (P1) – Navigation**: No dependency on other stories; requires only Setup and Foundational.
- **User Story 1 (P1) – Sales Overview**: Depends on navigation shell and shared HTTP/config; otherwise independent.
- **User Story 2 (P1) – Beers Catalog**: Depends on navigation shell and shared HTTP/config; otherwise independent.
- **User Story 3 (P1) – Orders**: Depends on navigation shell, shared HTTP/config, and the existence of Catalog/Beer models; can still be tested independently.
- **User Story 4 (P2) – Warehouse Snapshot**: Depends on navigation shell and shared HTTP/config; otherwise independent.

### Within Each User Story

- API clients and mappings should be implemented before or alongside view models.
- View models should be implemented before UI pages are wired up.
- Unit tests should cover view model behaviour and mapping logic and run as part of CI.

---

## Parallel Execution Examples

### User Story 1 – Sales Overview

- [P] `SalesApiClient` implementation in `Services/Api` (T016).
- [P] `SalesSummaryViewModel` implementation in `Features/Sales` (T017).
- [P] Mapping profile in `Services/Mapping` (T019).

### User Story 2 – Beers Catalog

- [P] `CatalogApiClient` (T021).
- [P] `CatalogViewModel` (T022).
- [P] Mapping profile (T024).

### User Story 3 – Orders

- [P] `OrdersApiClient` (T026).
- [P] `OrdersListViewModel` and `OrderDetailViewModel` (T027, T028).
- [P] Mapping profile (T030).

### User Story 4 – Warehouse Snapshot

- [P] `WarehouseApiClient` (T032).
- [P] `WarehouseViewModel` (T033).
- [P] Mapping profile (T035).

Multiple developers can work on different user stories in parallel once Phases 1–2 and the navigation shell (Phase 3) are complete.

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 (Setup) and Phase 2 (Foundational).
2. Implement Phase 3 (Navigation / US5) so the app is navigable.
3. Implement Phase 4 (Sales Overview / US1) as the first MVP slice.
4. Validate Sales Overview independently against the backend APIs.

### Incremental Delivery

- After MVP (US5 + US1), add:
  - Phase 5 (Catalog / US2).
  - Phase 6 (Orders / US3).
  - Phase 7 (Warehouse / US4).
- Each story is independently testable and can be demonstrated to stakeholders.

### Testing & Quality

- Ensure unit tests for every new view model and service (tasks T020, T025, T031, T036, T038).
- Keep mappings and API contracts in sync with `contracts/mobile-app-apis.openapi.yaml`.
- Use the quickstart to validate end-to-end setup before feature handoff.
