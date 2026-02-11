# Data Model: Brewery Mobile App

## Domain Entities

### Beer
- **Description**: Product brewed by the brewery.
- **Fields**:
  - `id` (string): Unique identifier/code.
  - `name` (string): Display name.
  - `style` (string): Beer style/category.
  - `abv` (number): Alcohol by volume (%).
  - `packaging` (string): Packaging type (e.g., keg, bottle, can).
  - `basePrice` (number): Base price per unit (currency handled by backend).
  - `isActive` (bool): Whether the beer is currently sold.
- **Relationships**:
  - Referenced by `OrderLineItem`.
- **Validation**:
  - `name`, `style`, and `packaging` required.
  - `abv` must be ≥ 0.

### SalesSummary
- **Description**: Aggregated sales metrics for a period and optional filters.
- **Fields**:
  - `period` (enum): e.g., Today, Last7Days, Last30Days, Custom.
  - `startDate` / `endDate` (date, optional for Custom).
  - `totalRevenue` (number).
  - `totalVolume` (number).
  - `topBeers` (array of `BeerSalesSummary`).
- **Related Types**:
  - `BeerSalesSummary`: beer id, name, quantity sold, revenue.

### Order
- **Description**: Sales order raised via the mobile app.
- **Fields**:
  - `id` (string): Order identifier.
  - `customerId` (string).
  - `customerName` (string): Denormalized for display.
  - `status` (enum): Draft, Confirmed, Shipped, Cancelled.
  - `createdAt` / `updatedAt` (datetime).
  - `lines` (array of `OrderLineItem`).
  - `totalQuantity` (number).
  - `totalAmount` (number).
- **State Transitions**:
  - Draft → Confirmed → Shipped.
  - Draft/Confirmed → Cancelled.
- **Validation**:
  - At least one `OrderLineItem` required.
  - All quantities > 0.

### OrderLineItem
- **Description**: Single beer in an order.
- **Fields**:
  - `beerId` (string).
  - `beerName` (string): Denormalized for display.
  - `quantity` (number).
  - `unitPrice` (number).
  - `lineTotal` (number).
- **Validation**:
  - `quantity` > 0.

### Customer
- **Description**: Party placing the order.
- **Fields**:
  - `id` (string).
  - `name` (string).
  - `contactInfo` (string/structured).
  - `type` (string): e.g., bar, restaurant, distributor.

### StockItem
- **Description**: Stock level for a beer at a specific location.
- **Fields**:
  - `beerId` (string).
  - `locationId` (string).
  - `locationName` (string).
  - `onHand` (number).
  - `reserved` (number).
- **Validation**:
  - `onHand` and `reserved` ≥ 0.

### User
- **Description**: Authenticated app user.
- **Fields**:
  - `id` (string).
  - `name` (string).
  - `role` (enum): OwnerManager, SalesRep, WarehouseStaff, Admin.
  - `permissions` (array of string): Optional, for fine-grained rules.

## App-Side View Models

### DashboardViewModel
- Combines latest `SalesSummary` and high-level stock/order indicators.

### SalesSummaryViewModel
- Wraps `SalesSummary` with UI-specific flags (loading state, selected period).

### CatalogViewModel
- Exposes filter/sort state, current page of `Beer` items.

### OrdersListViewModel
- Shows current and past orders, with filters for date range, status, and customer.

### OrderDetailViewModel
- Manages a single `Order` for creation/editing and validation feedback.

### WarehouseViewModel
- Shows `StockItem` list grouped by beer or by location.

## Mapping and DTOs

- External API DTOs will closely follow these shapes but may differ in naming.
- A dedicated mapping layer in `Services/Mapping/` will convert API DTOs to
  internal domain/view models and vice versa.
