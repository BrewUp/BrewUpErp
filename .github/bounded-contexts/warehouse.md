# Warehouse Bounded Context Skill

## Purpose

The Warehouse bounded context owns the physical handling of goods.

It is responsible for stock availability, warehouse movements, shipment preparation, dispatch readiness, and reorder signals.

Warehouse is the source of truth for physical inventory state.

## Core Language

Use Warehouse language consistently.

Preferred terms:

- Warehouse
- Stock
- Stock Item
- Stock Movement
- Stock Availability
- Reorder Threshold
- Shipment
- Shipment Preparation
- Ready for Dispatch
- Picking
- Packing
- Dispatch

Avoid generic or ambiguous terms such as:

- Product availability
- Order status
- Delivery document
- Inventory transaction
- Generic quantity

unless they already exist in the Warehouse model.

## Ownership Rules

Warehouse owns:

- physical stock
- stock availability
- stock movements
- shipment preparation
- picking and packing
- reorder threshold evaluation
- dispatch readiness

Warehouse does not own:

- customer demand
- sales order lifecycle
- commercial commitments
- product catalog definition
- production planning
- invoicing or accounting

Sales owns customer orders and demand.

Catalog owns product definitions.

Production owns production capacity and brewing plans.

Accounting owns invoices and financial records.

Do not directly access another module database.

## Architectural Rules

Respect the modular monolith boundaries.

Do not introduce direct dependencies from Warehouse to another module implementation.

Prefer integration through:

- domain events
- application services
- MCP exposed capabilities
- explicit contracts

Warehouse should expose physical fulfillment capabilities, not internal storage structures.

Never bypass bounded context boundaries for convenience.

## MCP Server Role

The Warehouse MCP Server exposes Warehouse capabilities to external agents and clients.

It should expose business capabilities, not database tables.

Good tool names:

- GetStockAvailability
- GetBeerAvailability
- GetStockMovements
- GetItemsBelowReorderThreshold
- PrepareShipment
- GetShipmentsReadyForDispatch
- GetShipmentPreparationStatus
- EvaluateStockOutRisk

Avoid technical tool names:

- QueryWarehouseTable
- GetWarehouseDbRows
- ExecuteWarehouseSql
- FetchStockData
- UpdateQuantityColumn

## Agent Reasoning Guidance

When reasoning about Warehouse, focus on physical availability and fulfillment feasibility.

Examples:

- "Is this beer available in stock?"
- "Which items are below reorder threshold?"
- "Can this shipment be prepared?"
- "Which shipments are ready for dispatch?"
- "What is the stock impact of this demand?"
- "Which beers risk going out of stock?"

If the question requires customer demand, Sales must provide it.

If the question requires product definition or beer metadata, Catalog must provide it.

If the question requires production capacity, Production must provide it.

Warehouse must not invent demand, product definitions, or production plans.

## What-if Scenarios

In what-if analysis, Warehouse provides physical stock impact.

Example:

User asks:

"What happens if customer ACME doubles its orders next month?"

Warehouse should answer:

- current stock availability
- stock impact of projected demand
- items at risk of stock-out
- reorder threshold impact
- fulfillment risk from the warehouse perspective

Warehouse should not answer:

- whether the customer will place the order
- commercial value of the order
- production feasibility
- invoice or payment impact

Those are responsibilities of other bounded contexts.

## Commands

Warehouse commands should express intentional warehouse actions.

Good commands:

- PrepareShipment
- RegisterStockMovement
- AdjustStockLevel
- ReserveStock
- ReleaseStockReservation
- MarkShipmentReadyForDispatch
- ReorderStock

Avoid commands that mix responsibilities:

- CompleteSalesOrder
- ConfirmCustomerOrder
- ProduceBeer
- InvoiceShipment

## Events

Warehouse events should express meaningful facts in the warehouse domain.

Good events:

- StockReceived
- StockAdjusted
- StockReserved
- StockReservationReleased
- StockMovementRegistered
- StockBelowReorderThreshold
- ShipmentPreparationStarted
- ShipmentPrepared
- ShipmentReadyForDispatch
- ShipmentDispatched

Avoid events that belong to other bounded contexts:

- SalesOrderConfirmed
- BeerProduced
- InvoiceIssued
- CustomerPaymentReceived

Warehouse may react to events from other contexts, but it should publish events in its own language.

## Invariants

Warehouse should protect invariants related to physical stock and fulfillment.

Examples:

- Stock quantity cannot become negative unless explicitly modeled as backorder.
- A shipment cannot be marked ready for dispatch before required items are prepared.
- Stock cannot be reserved if available quantity is insufficient.
- Reorder threshold evaluation must use Warehouse-owned stock state.
- Shipment preparation must be idempotent when retried.
- A dispatched shipment cannot be prepared again.

## Collaboration Rules

When Sales requests shipment preparation, Warehouse evaluates physical feasibility.

Sales provides:

- sales order identifier
- requested items
- requested quantities
- customer delivery commitment if relevant

Warehouse decides:

- whether items can be picked
- whether items can be packed
- whether shipment is ready for dispatch
- whether stock is insufficient

Warehouse should not change Sales order status directly.

It should publish Warehouse events that Sales can react to.

## MCP and Agent Collaboration

In agentic scenarios:

- SalesAgent provides demand.
- WarehouseAgent evaluates fulfillment and stock impact.
- ProductionAgent evaluates production capacity.
- Mother coordinates cross-context reasoning.

WarehouseAgent should answer only from Warehouse capabilities and MCP-exposed information.

If required information is outside Warehouse, it should request collaboration rather than inventing an answer.

## Design Principles

Warehouse models physical reality, not commercial intent.

A Warehouse capability should express a fulfillment or stock-related business question.

A Warehouse command should express an intentional warehouse operation.

A Warehouse event should express something that physically happened or became true in the warehouse.

## Codex Instructions

When modifying Warehouse code:

1. Preserve bounded context boundaries.
2. Use Warehouse language.
3. Do not introduce direct database access to Sales, Catalog, Production, or Accounting.
4. Keep MCP tools business-oriented.
5. Do not expose database tables as tools.
6. Do not make Warehouse responsible for customer demand or production planning.
7. Model shipment preparation as a Warehouse responsibility.
8. Use events to communicate cross-context facts.
9. If a feature crosses boundaries, propose collaboration through events, MCP tools, or agent-to-agent interaction.