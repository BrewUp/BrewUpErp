# Navigation Routes Contract

This document defines the logical navigation routes for the BrewApp MAUI Shell. It is not an HTTP API contract but serves as an internal contract between the shell, modules, and ViewModels.

## Route Overview

| Route                          | Description                          | Target Page                    |
|--------------------------------|--------------------------------------|--------------------------------|
| `//landing`                    | Application landing page             | `LandingPage`                  |
| `//modules/masterdata`         | MasterData module landing            | `MasterDataLandingPage`        |
| `//modules/sales`              | Sales module landing                 | `SalesLandingPage`             |
| `//modules/warehouse`          | Warehouse module landing             | `WarehouseLandingPage`         |
| `//modules/purchase`           | Purchase module landing              | `PurchaseLandingPage`          |

## Contract Rules

- All routes MUST be registered in `AppShell.xaml` (or equivalent Shell registration code).
- Module routes MUST use the `modules/{moduleKey}` pattern to stay consistent.
- Navigation services MUST use these route strings; hard-coded page type navigation is not allowed.
- Each module is responsible for registering additional internal routes under its `RoutePrefix` (e.g., `modules/sales/orders`).

## Responsibilities

- **BrewApp.Shell**: Owns registration of top-level routes and the landing page.
- **Module Projects**: Own registration of their internal routes and pages.
- **Shared Core**: Defines constants and helper classes for routes to avoid string duplication.

