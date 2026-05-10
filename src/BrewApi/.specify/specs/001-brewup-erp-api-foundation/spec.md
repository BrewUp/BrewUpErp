# Spec 001 — BrewUp ERP API: Greenfield Foundation

**Feature branch**: `001-brewup-erp-api-foundation`  
**Status**: Draft  
**Created**: 2026-05-10  
**Constitution**: `.specify/memory/constitution.md` v1.0.0

---

## 1. Overview

### 1.1 Problem statement

Craft breweries need a unified back-office system to manage their master data (customers, beers, warehouses), record sales orders, coordinate production workflows across departments, and consult operational dashboards — all through a single REST API that can be consumed by any front-end or third-party system.

Currently no automated system exists; brewery staff track customers and orders manually using spreadsheets and phone calls, leading to stock errors, delayed fulfillment, and lost revenue.

### 1.2 Vision

Build **BrewUp**, a modular-monolith ERP REST API for the craft-brewery domain that:

- Provides a clean, versioned HTTP API for managing brewery operations.
- Enforces domain boundaries between business capabilities (MasterData, Sales, Warehouse, Sagas, Dashboards).
- Supports event-sourced writes and projection-based reads.
- Emits integration events to decouple modules and support future service extraction.
- Is fully spec-driven: every endpoint, contract, and behaviour is defined here before any code is written.

### 1.3 Scope

This specification covers the **complete greenfield API surface** across all five modules. It intentionally excludes:

- Authentication / authorisation (out of scope for v1).
- Front-end clients or mobile apps.
- Reporting exports (PDF, Excel).
- Multi-tenancy.

---

## 2. Modules and Capabilities

### 2.1 MasterData

Manages the brewery's reference data. All other modules consume MasterData entities by ID and name.

#### 2.1.1 Customer management

**User stories**

- As a sales manager, I want to register a new customer so that I can associate sales orders with them.
- As a sales manager, I want to update a customer's details (name, VAT, address) so that records stay accurate.
- As a sales manager, I want to update a customer's properties (consumer level, budget limit, active status) so that sales rules can be applied dynamically.
- As a sales manager, I want to delete a customer so that obsolete records are removed.
- As a sales manager, I want to list all customers (paginated) so that I can browse the customer base.
- As a sales manager, I want to retrieve a single customer by ID so that I can review their details.

**Acceptance criteria**

- `POST /v1/masterdata/customers` requires `ragioneSociale` (not empty) and `partitaIva` (not empty); returns 201 + `Location` header.
- `PUT /v1/masterdata/customers/{customerId}` replaces full customer record; returns 202.
- `PATCH /v1/masterdata/customers/{customerId}` updates `consumerLevel`, `budgetLimit`, `isEnabled`; returns 202.
- `DELETE /v1/masterdata/customers/{customerId}` marks customer deleted; returns 202.
- `GET /v1/masterdata/customers` returns `PagedResult<CustomerJson>` using `pageNumber` and `pageSize` query params.
- `GET /v1/masterdata/customers/{customerId}` returns `CustomerJson` or 404.
- On customer creation, a `CustomerCreated` integration event is published.
- On customer update (PUT), a `CustomerUpdated` integration event is published.

#### 2.1.2 Beer (product) management

**User stories**

- As a production manager, I want to register a beer product so that it can be referenced in sales orders and warehouse stock.
- As a production manager, I want to update a beer's details (name, style, ABV, price).
- As a production manager, I want to list all beers (paginated).
- As a production manager, I want to retrieve a single beer by ID.

**Acceptance criteria**

- `POST /v1/masterdata/beers` requires `name` (not empty); returns 201 + `Location` header.
- `PUT /v1/masterdata/beers/{beerId}` replaces full beer record; returns 202.
- `GET /v1/masterdata/beers` returns `PagedResult<BeerJson>`.
- `GET /v1/masterdata/beers/{beerId}` returns `BeerJson` or 404.
- On beer creation, a `BeerCreated` integration event is published.
- On beer update, a `BeerUpdated` integration event is published.

#### 2.1.3 Warehouse registration

**User stories**

- As a warehouse manager, I want to register a warehouse location so that stock levels can be tracked per location.
- As a warehouse manager, I want to list all warehouses.

**Acceptance criteria**

- `POST /v1/masterdata/warehouses` requires `name` (not empty); returns 201 + `Location` header.
- `GET /v1/masterdata/warehouses` returns `PagedResult<WarehouseJson>`.

---

### 2.2 Sales

Manages the lifecycle of sales orders from creation to completion.

**User stories**

- As a salesperson, I want to create a sales order for a customer with one or more line items, so that the brewery knows what to produce and ship.
- As a salesperson, I want to close a sales order when all items have been fulfilled, so that the order is marked complete.
- As a salesperson, I want to list all sales orders (paginated) so that I can track open and closed orders.
- As a salesperson, I want to retrieve the details of a specific sales order.

**Acceptance criteria**

- `POST /v1/sales/salesorders` requires `customerId`, `customerName`, `salesOrderNumber`, `orderDate`, `deliveryDate`, and at least one line item (each with `beerId`, `beerName`, `quantity`, `price`); returns 201 + `Location` header.
- `PATCH /v1/sales/salesorders/{salesOrderId}/close` closes the order; returns 202.
- `GET /v1/sales/salesorders` returns `PagedResult<SalesOrderJson>`.
- `GET /v1/sales/salesorders/{salesOrderId}` returns `SalesOrderJson` or 404.
- On creation, a `SalesOrderCreated` integration event is published.
- On close, a `SalesOrderClosed` integration event is published.
- Budget verification: before a sales order is created, the system must verify that the customer has sufficient remaining budget (`VerifyCustomerBudget` command).

---

### 2.3 Warehouse

Manages stock levels and outbound shipments triggered by fulfilled sales orders.

**User stories**

- As a warehouse operator, I want to load stock into a warehouse location so that available inventory is tracked.
- As a warehouse operator, I want to reduce stock when items are shipped against a sales order.
- As a warehouse operator, I want to list all stock entries for a warehouse.
- As a warehouse operator, I want to view the stock level for a specific beer in a warehouse.

**Acceptance criteria**

- `POST /v1/warehouse/stock` requires `warehouseId`, `beerId`, `beerName`, `quantity`; returns 201 + `Location` header.
- `PATCH /v1/warehouse/stock/{stockId}/reduce` reduces stock by a given quantity; returns 202 or 409 if insufficient.
- `GET /v1/warehouse/stock` returns `PagedResult<StockJson>` optionally filtered by `warehouseId`.
- `GET /v1/warehouse/stock/{stockId}` returns `StockJson` or 404.
- Warehouse stock is updated automatically when a `SalesOrderCreated` integration event is received (stock reservation).
- A `StockReduced` integration event is published when stock is reduced.

---

### 2.4 Sagas

Orchestrates multi-step business workflows that span more than one module.

#### 2.4.1 Sales order fulfillment saga

**User story**

- As the system, I want to automatically orchestrate the fulfillment of a sales order (verify budget → reserve stock → confirm order → ship) so that no manual intervention is needed for standard orders.

**Acceptance criteria**

- The saga is triggered by a `SalesOrderCreated` integration event.
- Steps: (1) publish `VerifyCustomerBudget`; (2) on budget approved, publish stock reservation; (3) on stock confirmed, mark order as `InProgress`; (4) on shipment confirmed, close the order.
- If any step fails, the saga publishes a compensating event and the order moves to `Cancelled` state.
- The saga state is persisted in MongoDB.
- `GET /v1/sagas/salesorderfulfillment/{sagaId}` returns the current saga state.

---

### 2.5 Dashboards

Provides aggregated, read-only views for management reporting.

**User stories**

- As a manager, I want to see a summary of open vs. closed sales orders so that I can track business throughput.
- As a manager, I want to see total revenue by period (day, week, month) so that I can monitor financial performance.
- As a manager, I want to see current stock levels per warehouse so that I can plan procurement.

**Acceptance criteria**

- `GET /v1/dashboards/salesorders/summary` returns `SalesOrderSummaryJson` (open count, closed count, total order value).
- `GET /v1/dashboards/revenue` accepts `from` and `to` date query params; returns `RevenueJson`.
- `GET /v1/dashboards/warehouse/stock` returns `PagedResult<WarehouseStockSummaryJson>`.
- Dashboard data is eventually consistent — projections are updated from integration events; no real-time guarantees.
- All dashboard endpoints are **read-only** (GET only) and carry no mutation side effects.

---

## 3. Cross-Cutting Concerns

### 3.1 Integration event contracts

The following integration events flow between modules:

| Event | Published by | Consumed by |
|-------|-------------|-------------|
| `CustomerCreated` | MasterData | Sales (ACL), Sagas |
| `CustomerUpdated` | MasterData | Sales (ACL) |
| `BeerCreated` | MasterData | Warehouse (ACL) |
| `BeerUpdated` | MasterData | Warehouse (ACL) |
| `SalesOrderCreated` | Sales | Warehouse (ACL), Sagas, Dashboards |
| `SalesOrderClosed` | Sales | Sagas, Dashboards |
| `SalesOrderSagaStarted` | Sagas | Sales |
| `StockReduced` | Warehouse | Sagas, Dashboards |

### 3.2 Pagination

All list endpoints accept `pageNumber` (default: 1) and `pageSize` (default: 20, max: 100) query parameters and return `PagedResult<T>` with `totalCount`, `pageNumber`, `pageSize`, and `items`.

### 3.3 Validation rules

- All `Id` path parameters must be non-empty strings (UUID-compatible).
- All `name` / `ragioneSociale` fields must be non-empty strings ≤ 250 characters.
- Numeric quantities and amounts must be ≥ 0.
- Dates must be valid ISO-8601 format.

### 3.4 Error handling

All error responses use `application/problem+json` with an additional `timestamp` field (UTC ISO-8601).

| Scenario | HTTP status |
|----------|------------|
| Validation failure | 400 Bad Request |
| Resource not found | 404 Not Found |
| Business rule violation | 409 Conflict |
| Unexpected error | 500 Internal Server Error |

---

## 4. Non-Functional Requirements

| Requirement | Target |
|-------------|--------|
| API versioning | Path-based (`/v1/...`). Breaking changes require a new version segment. |
| Observability | Structured JSON logs (Serilog). Request/response correlation IDs. |
| OpenAPI | Full `openapi.yaml` maintained at repository root. |
| Health checks | `/health` endpoint returning overall and per-dependency status. |
| Cancellation | All async methods accept and propagate `CancellationToken`. |
| Idempotency | Command IDs derived from caller-supplied resource IDs (UUID v7). |

---

## 5. Out of Scope for This Specification

- Authentication (JWT, API key) — deferred to spec 002.
- Rate limiting and throttling — deferred.
- Multi-tenancy — deferred.
- Email notifications — deferred.
- Reporting exports (PDF/Excel) — deferred.
- Front-end / SPA — separate repository.

---

## 6. Glossary

| Term | Definition |
|------|-----------|
| **RagioneSociale** | Italian legal name of a company (customer legal name) |
| **PartitaIva** | Italian VAT registration number |
| **ConsumerLevel** | Classification of a customer's consumption tier (e.g., Teetotaler, Regular, Heavy) |
| **BudgetLimit** | Maximum cumulative order value allowed for a customer before a manual approval is required |
| **Saga** | A long-running, stateful business process orchestrating events across module boundaries |
| **ACL (Anti-Corruption Layer)** | A handler in a module's `Facade/Acl/` folder that translates incoming integration events into module-internal commands |
| **PagedResult** | Standard paginated response wrapper: `totalCount`, `pageNumber`, `pageSize`, `items` |

---

## 7. Open Questions

- [ ] [NEEDS CLARIFICATION] What is the exact `ConsumerLevel` enumeration? (e.g., `Teetotaler`, `Occasional`, `Regular`, `Heavy`)
- [ ] [NEEDS CLARIFICATION] Should the budget limit be enforced hard (reject order) or soft (warn only)?
- [ ] [NEEDS CLARIFICATION] Is the stock reservation in the Warehouse saga atomic or eventually consistent?
- [ ] [NEEDS CLARIFICATION] What is the maximum number of line items per sales order?
- [ ] [NEEDS CLARIFICATION] Should dashboard data use real-time aggregation or pre-computed projections only?

---

## 8. Specification Checklist

- [x] Problem statement is clear and measurable
- [x] All five modules are described
- [x] Every user story has acceptance criteria
- [x] Integration event table is complete
- [x] Cross-cutting concerns documented
- [x] Non-functional requirements stated
- [x] Glossary covers domain-specific terms
- [x] Open questions flagged with `[NEEDS CLARIFICATION]`
- [ ] Open questions resolved (pending stakeholder input)
