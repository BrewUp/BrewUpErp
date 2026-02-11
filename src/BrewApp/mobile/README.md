# BrewApp Mobile

.NET MAUI mobile application for BrewApp ERP brewery management.

## Overview

This mobile app provides brewery staff with on-the-go access to:
- **Dashboard**: High-level metrics and business overview
- **Sales**: View sales summaries and performance data
- **Catalog**: Browse and search the beers catalog
- **Orders**: Create and track sales orders
- **Warehouse**: View stock levels and inventory

## Technology Stack

- **.NET 10** / **.NET MAUI 10**
- **C#** with nullable reference types
- **System.Text.Json** for API serialization
- **HttpClient** for external API calls
- **xUnit** for unit testing

## Project Structure

```
mobile/src/
├── Configuration/       # API settings and app configuration
├── Features/            # Feature-organized pages and view models
│   ├── Dashboard/
│   ├── Sales/
│   ├── Catalog/
│   ├── Orders/
│   └── Warehouse/
├── Services/            # Shared services (API clients, mapping, auth)
│   ├── Api/
│   ├── Mapping/
│   └── Auth/
├── Components/          # Reusable UI components
├── Resources/           # Styles, fonts, images
├── Platforms/           # Platform-specific code (Android, iOS)
├── App.xaml(.cs)        # Application root
├── AppShell.xaml(.cs)   # Navigation shell
└── MauiProgram.cs       # Dependency injection and configuration
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- .NET MAUI 10 workloads (Android and iOS)
- Visual Studio 2022 17.12+ or VS Code with C# Dev Kit

### Configuration

1. Copy the example API settings file:
   ```bash
   cp src/Configuration/apiSettings.example.json src/Configuration/apiSettings.json
   ```

2. Update `apiSettings.json` with your environment's API endpoints and credentials:
   - `SalesBaseUrl`
   - `CatalogBaseUrl`
   - `OrdersBaseUrl`
   - `WarehouseBaseUrl`
   - `ApiKey`

   **Important**: Do not commit `apiSettings.json` to version control (it's included in `.gitignore`).

### Running the App

#### Using Visual Studio
1. Open `BrewApp.Mobile.csproj`
2. Select your target platform (Android or iOS)
3. Press F5 to build and run

#### Using CLI
```bash
# Android
dotnet build -t:Run -f net10.0-android

# iOS (on macOS)
dotnet build -t:Run -f net10.0-ios
```

### Running Tests

```bash
cd tests/unit
dotnet test
```

## Architecture

### API Communication

All data is fetched from external BrewUp ERP APIs:
- No local database
- Online-first with optional light caching
- Strongly-typed API clients per feature context

### Dependency Injection

Services are registered in `MauiProgram.cs`:
- Configuration (`ApiSettings`)
- HttpClient factory
- Feature-specific API clients
- View models (as needed)

### Navigation

MAUI Shell provides tab-based navigation between the 5 main contexts.  
Additional routes for detail pages can be registered in `AppShell.xaml.cs`.

## Development Guidelines

- Follow the existing namespace and folder structure
- Place feature-specific code in the corresponding `Features/` folder
- Use dependency injection for services
- Keep view models testable (no direct UI dependencies)
- Write unit tests for view models and services

## For More Information

- Feature specs: `../../specs/002-brewery-mobile-app/`
- API contracts: `../../specs/002-brewery-mobile-app/contracts/`
- Implementation tasks: `../../specs/002-brewery-mobile-app/tasks.md`
