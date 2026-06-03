# Sales Bounded Context Skill

## Purpose

The Sales bounded context owns the commercial lifecycle of customer demand.

It is responsible for understanding customers, sales orders, order status, commercial commitments, and demand-related information.

Sales is the source of truth for customer orders.

## Core Language

Use Sales language consistently.

Preferred terms:

- Customer
- Sales Order
- Sales Order Line
- Order Status
- Pending Order
- Open Order
- Confirmed Order
- Cancelled Order
- Shipment Request
- Commercial Commitment

Avoid generic or ambiguous terms such as:

- Client
- Buyer
- Request
- Document
- Transaction

unless they already exist in the Sales model.

## Ownership Rules

Sales owns:

- customer sales orders
- order lifecycle
- order status
- customer demand
- commercial order data

Sales does not own:

- physical stock
- warehouse movements
- production planning
- shipment execution
- accounting or invoicing

When stock availability is needed, ask Warehouse or Inventory through their exposed capabilities.

When production capacity is needed, ask Production.

Do not directly access another module database.

## Architectural Rules

Respect the modular monolith boundaries.

Do not introduce direct dependencies from Sales to another module implementation.

Prefer integration through:

- domain events
- application services
- MCP exposed capabilities
- explicit contracts

Never bypass the bounded context boundary for convenience.

## MCP Server Role

The Sales MCP Server exposes Sales capabilities to external agents and clients.

It should expose business capabilities, not database tables.

Good tool names:

- GetOpenSalesOrders
- GetPendingSalesOrders
- GetSalesOrderById
- GetCustomerSalesOrders
- GetSalesOrderSummary
- GetLateSalesOrders

Avoid technical tool names:

- QuerySalesTable
- GetSalesDbRows
- ExecuteSalesSql
- FetchData

## Agent Reasoning Guidance

When reasoning about Sales, focus on customer demand and commercial commitments.

Examples:

- "What orders are currently open?"
- "Which customers have pending orders?"
- "What is the status of this sales order?"
- "Which orders may require shipment?"
- "What demand signal should be sent to Inventory or Production?"

If the question requires stock, availability, or fulfillment feasibility, Sales should not invent the answer.

Sales can provide the demand signal.

Inventory or Warehouse must provide availability.

Production must provide capacity.

## What-if Scenarios

In what-if analysis, Sales provides demand impact.

Example:

User asks:

"What happens if customer ACME doubles its orders next month?"

Sales should answer:

- current customer order volume
- recent demand trend
- projected demand increase
- affected products
- commercial impact

Sales should not answer:

- whether stock is sufficient
- whether production can satisfy demand
- whether warehouse can ship on time

Those are responsibilities of other bounded contexts.

## Design Principles

Sales models business intent, not data access.

A Sales capability should express a business question.

A Sales event should express something meaningful that happened in the commercial domain.

A Sales command should express an intentional business action.

Good events:

- SalesOrderCreated
- SalesOrderConfirmed
- SalesOrderCancelled
- SalesOrderReadyForPreparation

Good commands:

- CreateSalesOrder
- ConfirmSalesOrder
- CancelSalesOrder
- RequestShipmentPreparation

## Codex Instructions

When modifying Sales code:

1. Preserve bounded context boundaries.
2. Use Sales language.
3. Do not introduce direct database access to other modules.
4. Prefer explicit application-level contracts.
5. Keep MCP tools business-oriented.
6. Do not make Sales responsible for Inventory, Warehouse, Production, or Accounting decisions.
7. If a feature crosses boundaries, propose collaboration through events, MCP tools, or agent-to-agent interaction.