# Master Data Bounded Context Skill

## Purpose

The Master Data bounded context owns stable reference information used across the ERP.

It is responsible for customers, suppliers, beers, products, styles, catalog data, and descriptive business information.

Master Data is the source of truth for shared business identities and reference data.

## Core Language

Use Master Data language consistently.

Preferred terms:

- Customer
- Supplier
- Beer
- Product
- Beer Style
- Catalog Item
- ABV
- SKU
- Reference Data

Avoid ambiguous terms such as:

- Client
- Vendor
- Item
- Thing
- Record
- Generic entity

unless they already exist in the Master Data model.

## Ownership Rules

Master Data owns:

- customer definitions
- supplier definitions
- beer definitions
- product catalog information
- beer styles
- product descriptive attributes
- ABV and catalog metadata
- shared identifiers

Master Data does not own:

- customer orders
- stock availability
- warehouse movements
- production planning
- shipment preparation
- invoices or payments

Sales owns customer demand and sales orders.

Warehouse owns physical stock and shipment preparation.

Production owns brewing and production capacity.

Accounting owns financial records.

Do not directly access another module database.

## Architectural Rules

Respect the modular monolith boundaries.

Do not introduce direct dependencies from Master Data to another module implementation.

Prefer integration through:

- domain events
- application services
- MCP exposed capabilities
- explicit contracts

Master Data should expose reference information, not internal storage structures.

## MCP Server Role

The Master Data MCP Server exposes stable business reference data to external agents and clients.

It should expose meaningful lookup capabilities, not database tables.

Good tool names:

- GetCustomers
- GetCustomerById
- GetSuppliers
- GetSupplierById
- GetBeers
- GetBeerById
- GetBeerStyles
- GetProducts
- GetProductById
- SearchCatalog

Avoid technical tool names:

- QueryMasterDataTable
- GetDbRows
- ExecuteSql
- FetchEntities
- ReadReferenceTable

## Agent Reasoning Guidance

When reasoning about Master Data, focus on identity, classification, and descriptive information.

Examples:

- "Who are our customers?"
- "Which beers are in the catalog?"
- "What is the ABV of this beer?"
- "Which products belong to this beer style?"
- "Who supplies this ingredient or product?"

Master Data should not infer operational state.

It can say what a product is.

It cannot say whether it is available in stock.

It can say who a customer is.

It cannot say what that customer ordered.

## What-if Scenarios

In what-if analysis, Master Data provides reference context.

Example:

User asks:

"What happens if customer ACME doubles its orders next month?"

Master Data may provide:

- customer identity
- customer classification
- related catalog items
- beer/product descriptions
- supplier relationships if relevant

Master Data should not answer:

- projected demand
- stock impact
- production feasibility
- commercial impact
- shipment risk

Those are responsibilities of other bounded contexts.

## Commands

Master Data commands should express intentional changes to reference data.

Good commands:

- RegisterCustomer
- UpdateCustomerDetails
- RegisterSupplier
- UpdateSupplierDetails
- AddBeerToCatalog
- UpdateBeerDetails
- AddProductToCatalog
- UpdateProductDetails
- DefineBeerStyle

Avoid commands that mix responsibilities:

- CreateSalesOrder
- ReserveStock
- PrepareShipment
- ScheduleProduction
- IssueInvoice

## Events

Master Data events should express meaningful facts about reference data.

Good events:

- CustomerRegistered
- CustomerDetailsUpdated
- SupplierRegistered
- SupplierDetailsUpdated
- BeerAddedToCatalog
- BeerDetailsUpdated
- ProductAddedToCatalog
- ProductDetailsUpdated
- BeerStyleDefined

Avoid events that belong to other bounded contexts:

- SalesOrderConfirmed
- StockReserved
- ShipmentReadyForDispatch
- BeerProduced
- InvoiceIssued

## Invariants

Master Data should protect invariants related to identity and reference consistency.

Examples:

- Customer identifiers must be unique.
- Supplier identifiers must be unique.
- Beer identifiers must be unique.
- Product identifiers must be unique.
- A beer must have a valid beer style when styles are modeled explicitly.
- ABV must be within a valid business range.
- A catalog item cannot reference an unknown product or beer.
- Reference data changes should not silently change historical operational facts in other contexts.

## Collaboration Rules

Other contexts may reference Master Data identities, but they should not own or modify Master Data records directly.

Sales may reference customers and products.

Warehouse may reference products or beers for stock items.

Production may reference beers or recipes.

Accounting may reference customers and suppliers for invoices.

If another context needs descriptive information, it should ask Master Data through an explicit capability.

If another context needs to react to reference data changes, Master Data should publish events.

## MCP and Agent Collaboration

In agentic scenarios:

- MasterDataAgent provides identity and reference context.
- SalesAgent provides demand and order context.
- WarehouseAgent provides stock and fulfillment context.
- ProductionAgent provides production capacity and brewing context.
- Mother coordinates cross-context reasoning.

MasterDataAgent should answer only from Master Data capabilities and MCP-exposed information.

If required information is outside Master Data, it should request collaboration rather than inventing an answer.

## Design Principles

Master Data models shared business vocabulary, not operational behavior.

A Master Data capability should answer a reference-data business question.

A Master Data command should express an intentional change to reference data.

A Master Data event should express that a reference fact has changed.

## Codex Instructions

When modifying Master Data code:

1. Preserve bounded context boundaries.
2. Use Master Data language.
3. Do not introduce direct database access to Sales, Warehouse, Production, or Accounting.
4. Keep MCP tools lookup-oriented and business-oriented.
5. Do not expose database tables as tools.
6. Do not make Master Data responsible for stock, orders, production, shipment, or accounting decisions.
7. Protect identity and catalog consistency.
8. Use events to communicate reference-data changes when other contexts may need to react.
9. If a feature crosses boundaries, propose collaboration through events, MCP tools, or agent-to-agent interaction.