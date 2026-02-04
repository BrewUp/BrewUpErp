# Quickstart: BrewApp Navigation Shell (.NET MAUI 10)

This guide explains how to set up, run, and extend the BrewApp navigation shell feature.

## 1. Prerequisites

- .NET 10 SDK (preview/appropriate version supporting .NET MAUI 10)
- .NET MAUI workload installed
- IDE: Visual Studio 2022+ (with MAUI support) or VS Code with appropriate extensions
- Access to the BrewUpErp repository

## 2. Solution Structure

The mobile solution is organized into multiple projects to enforce modular architecture:

- `BrewApp.Shell` (.NET MAUI App)
  - Startup, App.xaml, AppShell.xaml
  - Landing page and global navigation menu

- `BrewApp.Module.MasterData` (Class Library)
- `BrewApp.Module.Sales` (Class Library)
- `BrewApp.Module.Warehouse` (Class Library)
- `BrewApp.Module.Purchase` (Class Library)
  - Each contains its own Views, ViewModels, and registration logic

- `BrewApp.Shared.UI` (Class Library)
  - Shared controls, styles, resources

- `BrewApp.Shared.Core` (Class Library)
  - Navigation contracts, route constants, API interfaces, DI helpers

- `tests/*` projects
  - xUnit test projects per module + UI/navigation tests

## 3. Running the App

1. Open the solution in Visual Studio.
2. Set `BrewApp.Shell` as the startup project.
3. Select a target (Android emulator, iOS simulator, or device).
4. Run/debug the app.

Expected behavior:
- App launches to the landing page with a brewery-themed image.
- A navigation menu (flyout) is available, listing MasterData, Sales, Warehouse, and Purchase.
- Selecting a menu item navigates to the corresponding module placeholder screen.

## 4. Adding or Modifying Modules

To adjust existing modules (e.g., add functionality) or add new modules:

1. Create a new module project (e.g., `BrewApp.Module.Reporting`).
2. Add Views and ViewModels following MVVM with `CommunityToolkit.Mvvm`.
3. Define a `ModuleDescriptor` and `NavigationMenuItem` entry for the new module.
4. Register routes in Shell and update the navigation menu.
5. Add unit and UI tests in the corresponding tests project.

## 5. Navigation & MVVM Guidelines

- Use `CommunityToolkit.Mvvm` for ViewModels (observable properties, commands).
- Keep ViewModels free of direct UI types; use abstractions and services.
- Use Shell route navigation (`GoToAsync`) with constants from Shared.Core.
- Ensure every public ViewModel and navigation service is covered by unit tests.

## 6. Performance & Quality

- Profile startup time and navigation transitions on representative devices.
- Keep image assets optimized and use appropriate resolutions.
- Run analyzers and tests before committing.
- Follow the BrewApp Mobile Constitution for all changes.

