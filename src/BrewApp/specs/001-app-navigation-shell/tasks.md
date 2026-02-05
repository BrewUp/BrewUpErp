---

description: "Tasks for BrewApp Navigation Shell (.NET MAUI 10, MVVM, Modular)"
---

# Tasks: BrewApp Navigation Shell

**Input**: Design documents from `../001-app-navigation-shell/`  
**Prerequisites**: plan.md (filled), spec.md, research.md, data-model.md, quickstart.md, contracts/navigation-routes.md

**Tests**: This feature MUST follow Test-First Development per Constitution. Test tasks are included and MUST be completed before implementation tasks for each story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize the .NET MAUI 10 solution, modular projects, and basic tooling.

- [X] T001 Create solution directory and base MAUI app project `BrewApp.Shell` in `BrewUpErp.Mobile/src/BrewApp.Shell/` using `dotnet new maui`.
- [X] T002 Create shared core class library project `BrewApp.Shared.Core` in `BrewUpErp.Mobile/src/BrewApp.Shared.Core/`.
- [X] T003 Create shared UI class library project `BrewApp.Shared.UI` in `BrewUpErp.Mobile/src/BrewApp.Shared.UI/`.
- [X] T004 Create feature module class library projects `BrewApp.Module.MasterData`, `BrewApp.Module.Sales`, `BrewApp.Module.Warehouse`, `BrewApp.Module.Purchase` under `BrewUpErp.Mobile/src/`.
- [X] T005 [P] Create solution file `BrewUpErp.Mobile/BrewUpErp.Mobile.sln` and add all projects to the solution.
- [ ] T006 [P] Add test projects `BrewApp.Shell.Tests`, `BrewApp.Module.MasterData.Tests`, `BrewApp.Module.Sales.Tests`, `BrewApp.Module.Warehouse.Tests`, `BrewApp.Module.Purchase.Tests`, `BrewApp.UITests` under `BrewUpErp.Mobile/tests/` and reference corresponding projects.
- [ ] T007 [P] Configure `Directory.Build.props` and `Directory.Build.targets` in `BrewUpErp.Mobile/` for shared settings (nullable enable, analyzers, language version C# 13).
- [ ] T008 [P] Add CI configuration (YAML or pipeline) in `.github/workflows/brewapp-navigation-shell.yml` to build all projects and run tests.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T009 Setup .NET analyzers and StyleCop ruleset in `BrewUpErp.Mobile/` and reference from all projects.
- [ ] T010 [P] Add `CommunityToolkit.Mvvm` package reference to `BrewApp.Shell`, `BrewApp.Shared.Core`, and all `BrewApp.Module.*` projects.
- [ ] T011 [P] Implement base navigation contracts in `BrewUpErp.Mobile/src/BrewApp.Shared.Core/Navigation/Routes.cs` using constants from `contracts/navigation-routes.md`.
- [ ] T012 [P] Implement `INavService` interface and `ShellNavigationService` in `BrewUpErp.Mobile/src/BrewApp.Shared.Core/Navigation/`.
- [ ] T013 [P] Configure dependency injection in `BrewUpErp.Mobile/src/BrewApp.Shell/MauiProgram.cs` to register `ShellNavigationService` and shared services.
- [ ] T014 [P] Create base styles and color resources in `BrewUpErp.Mobile/src/BrewApp.Shared.UI/Resources/Styles.xaml` and link them into `BrewApp.Shell/App.xaml`.
- [ ] T015 [P] Configure unit test infrastructure (xUnit + Coverlet) in `BrewUpErp.Mobile/tests/BrewApp.Shell.Tests/` and shared test props.
- [ ] T016 [P] Setup UI test harness in `BrewUpErp.Mobile/tests/BrewApp.UITests/` to run basic navigation flows.

**Checkpoint**: Foundation ready – shell, navigation contracts, DI, styling, and tests infrastructure in place.

---

## Phase 3: User Story 1 - Launch App and View Landing Page (Priority: P1) 🎯 MVP

**Goal**: User launches the app and sees a brewery-branded landing page with image and basic branding.

**Independent Test**: Launch app on device/emulator and verify startup time <2s and landing page layout across device sizes.

### Tests for User Story 1 (NON-NEGOTIABLE) ⚠️

- [ ] T017 [P] [US1] Add unit tests for `LandingPageViewModel` in `BrewUpErp.Mobile/tests/BrewApp.Shell.Tests/ViewModels/LandingPageViewModelTests.cs` covering title, subtitle, hero image, and offline state.
- [ ] T018 [P] [US1] Add UI test to verify app launches to landing page with hero image in `BrewUpErp.Mobile/tests/BrewApp.UITests/Launch/LandingPageLaunchTests.cs`.
- [ ] T019 [P] [US1] Add performance test to measure cold start time in `BrewUpErp.Mobile/tests/BrewApp.UITests/Performance/StartupPerformanceTests.cs`.

### Implementation for User Story 1

- [X] T020 [P] [US1] Implement `LandingPageViewModel` in `BrewUpErp.Mobile/src/BrewApp.Shell/ViewModels/LandingPageViewModel.cs` using `CommunityToolkit.Mvvm`.
- [X] T021 [P] [US1] Implement `LandingPage.xaml` and `LandingPage.xaml.cs` in `BrewUpErp.Mobile/src/BrewApp.Shell/Views/LandingPage.xaml` with hero image, title, subtitle, and basic layout.
- [X] T022 [US1] Configure `App.xaml` and `App.xaml.cs` in `BrewUpErp.Mobile/src/BrewApp.Shell/App.xaml` to set Shell as MainPage and route startup to `LandingPage`.
- [X] T023 [US1] Add placeholder brewery image asset(s) in `BrewUpErp.Mobile/src/BrewApp.Shell/Resources/Images/` and bind from `LandingPage`.
- [ ] T024 [US1] Implement offline indication (e.g., banner or icon) in `LandingPage.xaml` bound to `IsOffline` in `LandingPageViewModel`.
- [ ] T025 [US1] Ensure responsive layout using MAUI layout containers and safe area handling for different device sizes in `LandingPage.xaml`.

### Quality Gate for User Story 1

- [ ] T026 [US1] Run analyzers and fix all warnings in `BrewApp.Shell` related to landing page and view model.
- [ ] T027 [US1] Verify code coverage ≥80% for `LandingPageViewModel` in `BrewApp.Shell.Tests`.
- [ ] T028 [US1] Run startup performance test and confirm cold start <2s on target device profile.

**Checkpoint**: User Story 1 complete – app launches to a responsive landing page with image and branding.

---

## Phase 4: User Story 2 - Navigate Between Module Sections (Priority: P1)

**Goal**: User can open a navigation menu and move between MasterData, Sales, Warehouse, and Purchase module placeholder screens, and return to Home.

**Independent Test**: From landing page, open the navigation menu and navigate to each module placeholder and back to Home without errors.

### Tests for User Story 2 (NON-NEGOTIABLE) ⚠️

- [ ] T029 [P] [US2] Add unit tests for `NavigationMenuItem` and `ModuleDescriptor` logic in `BrewUpErp.Mobile/tests/BrewApp.Shared.Core.Tests/Navigation/NavigationModelTests.cs`.
- [ ] T030 [P] [US2] Add unit tests for `ShellNavigationService` navigation methods in `BrewUpErp.Mobile/tests/BrewApp.Shared.Core.Tests/Navigation/ShellNavigationServiceTests.cs`.
- [ ] T031 [P] [US2] Add UI tests for navigating from landing page to each module and back to home in `BrewUpErp.Mobile/tests/BrewApp.UITests/Navigation/ModuleNavigationTests.cs`.

### Implementation for User Story 2

- [X] T032 [P] [US2] Define module descriptors and menu items for MasterData, Sales, Warehouse, and Purchase in `BrewUpErp.Mobile/src/BrewApp.Shared.Core/Navigation/ModulesCatalog.cs`.
- [X] T033 [P] [US2] Implement `AppShell.xaml` and `AppShell.xaml.cs` in `BrewUpErp.Mobile/src/BrewApp.Shell/AppShell.xaml` with Shell flyout containing routes `//landing`, `//modules/masterdata`, `//modules/sales`, `//modules/warehouse`, `//modules/purchase`.
- [X] T034 [P] [US2] Implement generic `ModulePlaceholderPage.xaml` and `ModulePlaceholderPage.xaml.cs` in `BrewUpErp.Mobile/src/BrewApp.Shell/Views/ModulePlaceholderPage.xaml` bound to `ModulePlaceholderViewModel`.
- [X] T035 [P] [US2] Implement `ModulePlaceholderViewModel` in `BrewUpErp.Mobile/src/BrewApp.Shell/ViewModels/ModulePlaceholderViewModel.cs` to show module name and description.
- [ ] T036 [P] [US2] In each module project (`BrewApp.Module.MasterData`, `BrewApp.Module.Sales`, `BrewApp.Module.Warehouse`, `BrewApp.Module.Purchase`), add a registration class (e.g., `ModuleMasterDataRegistration.cs`) under `Registration/` to register their primary routes and any future pages.
- [ ] T037 [US2] Wire module registration into `MauiProgram.cs` so that on startup all modules register their routes with Shell using the shared route constants.
- [X] T038 [US2] Add a "Home" entry (or logo tap behavior) in `AppShell.xaml` to navigate back to `//landing` from any module.
- [ ] T039 [US2] Ensure smooth transitions and appropriate animations by configuring Shell navigation and animation properties in `AppShell.xaml`.

### Quality Gate for User Story 2

- [ ] T040 [US2] Run analyzers and fix all warnings in `BrewApp.Shell`, `BrewApp.Shared.Core`, and module projects related to navigation and routes.
- [ ] T041 [US2] Verify code coverage ≥80% for navigation-related code (`ShellNavigationService`, route catalog, placeholder view models).
- [ ] T042 [US2] Run UI tests to confirm all module navigation paths work and no crashes occur when rapidly switching modules.

**Checkpoint**: User Story 2 complete – user can navigate between all module sections and Home.

---

## Phase 5: User Story 3 - Consistent Navigation Experience Across Platforms (Priority: P2)

**Goal**: Navigation feels native and consistent on both iOS and Android, while maintaining shared behavior and identity.

**Independent Test**: Run the app on iOS and Android; verify navigation patterns align with platform guidelines and feature parity is maintained.

### Tests for User Story 3 (NON-NEGOTIABLE) ⚠️

- [ ] T043 [P] [US3] Add UI tests that run on both iOS and Android to verify navigation menu behavior matches platform expectations in `BrewUpErp.Mobile/tests/BrewApp.UITests/Platforms/PlatformNavigationBehaviorTests.cs`.
- [ ] T044 [P] [US3] Add accessibility tests (screen reader labels, focus order, touch target sizes) for navigation elements in `BrewUpErp.Mobile/tests/BrewApp.UITests/Accessibility/NavigationAccessibilityTests.cs`.

### Implementation for User Story 3

- [ ] T045 [P] [US3] Apply platform-specific Shell styling and behavior in `BrewUpErp.Mobile/src/BrewApp.Shell/Platforms/*/AppShell.Platform.xaml` or platform partials to align with iOS HIG and Material Design.
- [ ] T046 [P] [US3] Ensure all navigation elements (flyout items, buttons, icons) have appropriate `AutomationProperties.Name` and accessibility labels in `AppShell.xaml` and `LandingPage.xaml`.
- [ ] T047 [US3] Validate safe area and notch handling on iOS and cutout areas on Android by adjusting layout in `LandingPage.xaml` and `AppShell.xaml`.
- [ ] T048 [US3] Externalize all navigation-related strings (menu item labels, titles) into resource files under `BrewUpErp.Mobile/src/BrewApp.Shell/Resources/Strings/`.

### Quality Gate for User Story 3

- [ ] T049 [US3] Run platform-specific UI tests on iOS and Android targets to confirm consistent behavior and passing accessibility checks.
- [ ] T050 [US3] Review navigation UX with design/stakeholders and record any deviations or agreed exceptions.

**Checkpoint**: User Story 3 complete – navigation experience is consistent and accessible across platforms.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final refinements, documentation, and cross-cutting improvements.

- [ ] T051 Add XML documentation comments for public navigation services and route constants in `BrewUpErp.Mobile/src/BrewApp.Shared.Core/Navigation/`.
- [ ] T052 Add ADR documenting the decision to use MAUI Shell + CommunityToolkit.Mvvm in `BrewUpErp.Mobile/docs/adr/0001-navigation-shell-and-mvvm.md`.
- [ ] T053 Add README section for mobile app architecture and navigation shell in `BrewUpErp.Mobile/README.md`.
- [ ] T054 Run full test suite (unit, integration, UI) across all projects and platforms and address any remaining failures.
- [ ] T055 Perform final performance profiling for startup and navigation and record results in `BrewUpErp.Mobile/docs/performance/navigation-shell-baseline.md`.

---

## Dependencies & Story Order

- User Story 1 (US1) depends on completion of Phases 1–2.
- User Story 2 (US2) depends on US1 (landing page and shell wiring) and foundational navigation contracts.
- User Story 3 (US3) depends on US1 and US2 (navigation behavior must exist before platform refinements).
- Polish phase depends on all user stories being implemented.

---

## Parallel Execution Examples

- In **Phase 1**, T005–T008 can run in parallel once base projects (T001–T004) exist.
- In **Phase 2**, T010–T016 can run in parallel across different projects (core, UI, tests).
- In **Phase 3 (US1)**, tests (T017–T019) can be implemented in parallel, followed by parallel implementation tasks (T020–T023) before integration and quality gate.
- In **Phase 4 (US2)**, navigation model tests (T029–T031) and implementations (T032–T036) can proceed in parallel by different developers.
- In **Phase 5 (US3)**, platform styling (T045), accessibility improvements (T046–T048), and platform UI tests (T043–T044) can be parallelized.

---

## Implementation Strategy

- Focus first on delivering a working **MVP** consisting of Phases 1–3 (US1) and 4 (US2): a MAUI app that launches to a landing page and lets users navigate between module placeholders.
- Treat **US3** (platform-specific polish) and **Phase 6** (polish & cross-cutting) as follow-on iterations that enhance UX, accessibility, and documentation without blocking core functionality.
- Maintain a strict **test-first** discipline: implement tests for each story and component before writing production code.
- Keep modules decoupled by using shared route constants and navigation services in `BrewApp.Shared.Core`, avoiding direct references between module implementations.
