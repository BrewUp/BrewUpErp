# Quickstart: Brewery Mobile App (.NET MAUI)

## Prerequisites

- .NET 10 SDK installed
- .NET MAUI 10 workloads installed (Android/iOS)
- Access to BrewUp ERP external APIs (dev/test environment)

## Project Structure (planned)

- `mobile/src/Configuration/apiSettings.json` – API endpoints and settings
- `mobile/src/Features/*` – Feature folders (Sales, Catalog, Orders, Warehouse, Dashboard)
- `mobile/src/Services/Api` – Typed API clients
- `mobile/src/Services/Mapping` – DTO ↔ view model mappers
- `mobile/ tests/` – Unit and UI tests

## Configure API Settings

1. Copy the example settings file:
   - From: `specs/002-brewery-mobile-app/contracts/api-settings.example.json`
   - To: `mobile/src/Configuration/apiSettings.json` (once the mobile project exists)
2. Update values for your environment:
   - `SalesBaseUrl`, `CatalogBaseUrl`, `OrdersBaseUrl`, `WarehouseBaseUrl`
   - `ApiKey` or other auth tokens (avoid committing real secrets)
3. Ensure the `ApiSettings` class in `Configuration/ApiSettings.cs` binds to the JSON section `ApiSettings`.

## Running the App (once project is created)

1. Restore and build the solution including the `mobile` project.
2. Run on an Android emulator or iOS simulator:
   - Example: `dotnet build` followed by running from your IDE with the desired target.
3. Log in with test credentials and verify:
   - Sales dashboard shows data from the configured API.
   - Catalog, Orders, and Warehouse screens load without errors.

## Running Tests

- Run unit tests (once created) using your test runner of choice (e.g., `dotnet test`).
- Add tests for new view models and services when implementing features.
