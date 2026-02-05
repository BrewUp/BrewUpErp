# Research: BrewApp Navigation Shell

## Overview

This document captures key technical decisions and alternatives for implementing the BrewApp navigation shell using .NET MAUI 10 with a modular architecture and MVVM pattern. All data remains in external APIs; this feature focuses on app structure, navigation, and UX/performance fundamentals.

---

## Decision 1: Navigation Framework (.NET MAUI Shell vs. Traditional NavigationPage)

**Decision**: Use .NET MAUI `Shell` as the primary navigation framework for BrewApp.

**Rationale**:
- Shell provides a declarative way to define routes, flyout menus, and tabs, matching the need for a global navigation menu with multiple modules.
- Built-in support for URI-based navigation simplifies mapping routes to module pages.
- Shell offers consistent patterns for flyout (hamburger) navigation and tabbed navigation across platforms.
- Reduces boilerplate compared to manual `NavigationPage` stacks and custom menu implementations.

**Alternatives considered**:
- **NavigationPage + Custom Flyout**: More control but higher complexity; would require manual state management and custom menu components. Rejected for initial scaffolding due to unnecessary complexity.
- **Third-party navigation frameworks (e.g., Prism for MAUI)**: Powerful but adds additional dependencies and learning curve; may be considered later if requirements grow. For now, built-in Shell is sufficient.

---

## Decision 2: MVVM Implementation Library

**Decision**: Use `CommunityToolkit.Mvvm` for MVVM infrastructure (INotifyPropertyChanged, commands, dependency injection helpers).

**Rationale**:
- Official Microsoft toolkit, well-supported and actively maintained.
- Lightweight and integrates naturally with MAUI and .NET DI.
- Reduces boilerplate for ViewModels (observable properties, commands) and improves readability.
- Aligns with Constitution Principle IV (Code Quality) by encouraging clean separation of concerns.

**Alternatives considered**:
- **Prism for MAUI**: Provides more advanced navigation and modularity features but adds complexity; may be revisited if future requirements demand region management or more advanced composition.
- **ReactiveUI**: Powerful reactive MVVM framework, but introduces a different programming model (Rx) which may be overkill for this project.
- **Hand-rolled MVVM**: Maximum control but leads to repeated boilerplate and higher maintenance cost.

---

## Decision 3: Solution & Project Structure (Modular Architecture)

**Decision**: Organize the solution into a shell app project, per-module class library projects, and shared libraries for UI and core logic.

**Rationale**:
- Satisfies Constitution Principle I by making each module independently buildable and testable.
- Enables team-based ownership per module (MasterData, Sales, Warehouse, Purchase).
- Simplifies targeted testing and deployment of module-specific features.
- Encourages clear public contracts between shell, shared core, and modules via interfaces and route definitions.

**Alternatives considered**:
- **Single MAUI project with folder-based separation**: Simpler initially but violates the modular architecture principle and leads to tight coupling.
- **Multiple full MAUI app projects**: Overkill; would complicate deployment and user experience (multiple apps instead of one unified BrewApp).

---

## Decision 4: Data Access Strategy for This Feature

**Decision**: Abstract API integration behind interfaces in `BrewApp.Shared.Core`, but do not implement actual API calls in this feature.

**Rationale**:
- This feature only needs placeholders and navigation; real data flows belong to future module-specific specs.
- Interfaces (e.g., `IMasterDataApi`, `ISalesApi`) can be defined now to guide module contracts without coupling to a specific HTTP library.
- Keeps navigation shell independent of API implementation details, in line with separation of concerns.

**Alternatives considered**:
- **Implement HttpClient/Refit clients now**: Unnecessary for scaffolding and risks premature design decisions without full requirements.
- **Mock data baked into views**: Quick for demos but risks polluting UI with test data paths; better to use ViewModels and injectable services.

---

## Decision 5: Testing Strategy

**Decision**: Use xUnit for unit and integration tests, plus MAUI test harness (or platform UI tests) for navigation and UX flows.

**Rationale**:
- xUnit is widely used in .NET, integrates well with .NET tooling, and supports parallelization.
- Coverlet + report generators provide coverage metrics needed for Constitution requirements.
- UI tests will focus on critical navigation flows: app launch, opening menu, switching modules, and returning home.

**Alternatives considered**:
- **NUnit or MSTest**: Also valid; xUnit chosen for consistency with many modern .NET projects but can be swapped if organization has a standard.
- **No UI tests**: Rejected; Constitution requires UI/UX and performance testing.

---

## Decision 6: Navigation Menu Pattern

**Decision**: Use a Shell flyout menu listing the main modules, with potential bottom tabs for future frequently-used flows.

**Rationale**:
- Flyout provides a familiar pattern for multi-module business apps (hamburger menu with sections).
- Adapts well to both iOS and Android while allowing platform-specific styling.
- Easy to extend with additional items (e.g., Settings, Profile) later.

**Alternatives considered**:
- **Bottom tab bar only**: Better for a small number of primary sections but may not scale well if more modules are added.
- **Pure tabbed navigation**: Less appropriate for a business app with multiple operational modules.

---

## Open Questions / Future Clarifications

- Exact branding assets (logos, color palette, typography) for the landing page and navigation.
- Final decision on supported desktop targets (Windows/macOS) beyond mobile.
- Organizational preference for test framework (xUnit vs NUnit vs MSTest) if standards exist.
- Any security requirements impacting navigation (e.g., role-based visibility of modules) once authentication is introduced.
