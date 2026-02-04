# Implementation Plan: BrewApp Navigation Shell

**Branch**: `001-app-navigation-shell` | **Date**: 2026-02-04 | **Spec**: ../001-app-navigation-shell/spec.md
**Input**: Feature specification from `../001-app-navigation-shell/spec.md`

## Summary

Implement the initial navigation shell for the BrewApp mobile application using .NET MAUI targeting .NET 10. This feature delivers the app startup experience, a brewery-branded landing page, and a cross-platform navigation menu that routes to four module placeholders: MasterData, Sales, Warehouse, and Purchase. All business data is accessed via existing external APIs; this feature focuses solely on the app scaffolding, navigation structure, and adherence to the modular architecture, MVVM pattern, and BrewApp Mobile Constitution.

## Technical Context

**Language/Version**: C# 13 (C# for .NET 10), .NET MAUI (NET 10 mobile targets)  
**Primary Dependencies**: .NET MAUI Shell, .NET Generic Host/DI, CommunityToolkit.Mvvm (MVVM helpers), HttpClient/Refit for API clients (future features), xUnit + FluentAssertions for tests  
**Storage**: No local storage for this feature; all data accessed via external REST APIs (existing MasterData, Sales, Warehouse, Purchase services)  
**Testing**: xUnit test projects per module (unit + integration of navigation logic); MAUI test framework / UI test harness for navigation flows; coverage enforced via Coverlet  
**Target Platform**: iOS 15+, Android 8.0+ (primary); Windows/macOS as optional desktop targets if supported by MAUI  
**Project Type**: Mobile (multi-project solution: app shell + per-module libraries + shared libraries)  
**Performance Goals**: App cold start <2s, warm start <1s; navigation transitions <300ms p95; 60fps for navigation animations and scrolling in menus; memory footprint for shell/navigation ≤150MB on target devices  
**Constraints**: Must follow modular architecture (each module as separate project), MVVM pattern throughout, no direct coupling between modules (shared abstractions only), offline launch must still show landing page and navigation shell even if APIs unavailable  
**Scale/Scope**: 1 app shell project, 4 feature modules (MasterData, Sales, Warehouse, Purchase), 1 shared UI/design system module, 1 shared core module (navigation, configuration, API abstractions). Initial scope ~8–12 screens (landing + 1 placeholder per module + basic error/offline states).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Review compliance with BrewApp Mobile Constitution principles:

- [x] **Modular Architecture**: Feature defined as multi-project MAUI solution with separate projects for each module and shared libraries; no monolithic app project.
- [x] **Test-First Development**: Plan includes unit/integration/UI test projects and mandates TDD for navigation logic and shell behavior.
- [x] **UX Consistency**: Navigation and landing page designed around shared design system module, MAUI standard controls, and platform guidelines (iOS HIG / Material Design).
- [x] **Code Quality**: Plan uses analyzers, StyleCop, and code review gates; navigation services and module contracts documented.
- [x] **Performance Requirements**: Performance baselines for startup, transition times, and memory explicitly included; profiling and automated checks planned.

**Violations**: None identified for this feature. Any future deviation (e.g., non-modular shortcut) MUST be justified with an ADR and remediation plan.

## Project Structure

### Documentation (this feature)

```text
specs/001-app-navigation-shell/
├── spec.md              # Feature specification
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── navigation-routes.md
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (solution root)

```text
BrewUpErp.Mobile/
├── src/
│   ├── BrewApp.Shell/                     # .NET MAUI app project (startup, AppShell)
│   │   ├── App.xaml
│   │   ├── AppShell.xaml                  # Shell with routes and navigation menu
│   │   ├── Views/
│   │   │   └── LandingPage.xaml
│   │   ├── ViewModels/
│   │   │   └── LandingPageViewModel.cs
│   │   └── Services/
│   │       └── Navigation/
│   │           └── ShellNavigationService.cs
│   ├── BrewApp.Module.MasterData/         # Feature module - MasterData
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   └── ModuleMasterDataRegistration.cs
│   ├── BrewApp.Module.Sales/              # Feature module - Sales
│   ├── BrewApp.Module.Warehouse/          # Feature module - Warehouse
│   ├── BrewApp.Module.Purchase/           # Feature module - Purchase
│   ├── BrewApp.Shared.UI/                 # Shared design system (controls, styles, templates)
│   └── BrewApp.Shared.Core/               # Shared core (navigation contracts, API abstractions, DI setup)
└── tests/
    ├── BrewApp.Shell.Tests/               # Unit + integration tests for shell & navigation
    ├── BrewApp.Module.MasterData.Tests/
    ├── BrewApp.Module.Sales.Tests/
    ├── BrewApp.Module.Warehouse.Tests/
    ├── BrewApp.Module.Purchase.Tests/
    └── BrewApp.UITests/                   # Cross-module navigation & UX tests
```

**Structure Decision**: Adopt the multi-module mobile architecture as the default for BrewApp. Each functional area (MasterData, Sales, Warehouse, Purchase) is implemented as a separate module project with its own Views, ViewModels, and tests. The `BrewApp.Shell` app project is responsible for startup, dependency injection configuration, and Shell-based navigation. Shared UI and core logic live in dedicated shared projects. This structure fully complies with Constitution Principle I (Modular Architecture) and supports MVVM by keeping view models and views organized per module.

## Complexity Tracking

> No constitution violations identified. Complexity remains manageable with clear module boundaries and shared infrastructure. If future changes introduce cross-module coupling or performance trade-offs, they MUST be recorded as ADRs and noted here.
