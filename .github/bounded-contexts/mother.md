# Mother Coordination Context Skill

## Purpose

Mother is not a business bounded context.

Mother is a coordination and reasoning context.

Its responsibility is to orchestrate collaboration between bounded contexts and agents when a question cannot be answered by a single context.

Mother does not own business data.

Mother owns reasoning and execution planning.

## Core Language

Use Mother language consistently.

Preferred terms:

- Agent
- Capability
- Execution Plan
- Execution Step
- Reasoning
- Collaboration
- Coordination
- Context Discovery
- Capability Discovery
- What-If Analysis
- Impact Analysis
- Cross-Context Reasoning

Avoid business terms that belong to other bounded contexts:

- Sales Order
- Shipment
- Stock
- Production Batch
- Invoice

Those belong to Sales, Warehouse, Production, and Accounting.

## Ownership Rules

Mother owns:

- execution plans
- reasoning workflows
- agent coordination
- capability discovery
- cross-context orchestration
- impact analysis
- what-if analysis

Mother does not own:

- customers
- products
- orders
- stock
- production schedules
- invoices
- payments

Mother must never become a source of truth for business data.

Business truth always belongs to a bounded context.

## Architectural Rules

Mother is an orchestrator.

Mother must not contain duplicated business logic from other bounded contexts.

Mother should coordinate.

Mother should not decide business rules on behalf of Sales, Warehouse, Production, or Accounting.

Whenever possible:

- ask the owning context
- collect signals
- correlate results
- generate conclusions

Never bypass bounded context ownership.

## MCP Role

Mother consumes MCP capabilities.

Mother is primarily a client of MCP servers.

Mother discovers capabilities exposed by:

- Sales MCP Server
- Warehouse MCP Server
- Production MCP Server
- Master Data MCP Server
- future MCP Servers

Mother should reason over capabilities, not implementations.

Mother must never assume internal implementation details.

## Agent Collaboration

Mother coordinates agents.

Examples:

- SalesAgent
- WarehouseAgent
- ProductionAgent
- MasterDataAgent

Mother may:

- select agents
- delegate tasks
- collect responses
- correlate information
- generate execution plans

Mother should not perform specialized business reasoning that belongs to an agent.

## Execution Planning

Mother should model work as execution plans.

An execution plan consists of:

- objective
- execution steps
- assigned agents
- collected results

Example:

Question:

"What happens if customer ACME doubles its orders next month?"

Execution Plan:

1. Ask SalesAgent for demand impact.
2. Ask WarehouseAgent for stock impact.
3. Ask ProductionAgent for capacity impact.
4. Correlate findings.
5. Generate final assessment.

Mother owns the plan.

Agents own the domain reasoning.

## What-If Analysis

Mother is responsible for predictive and cross-context analysis.

Typical questions:

- What happens if demand increases?
- What happens if production is delayed?
- What happens if a supplier cannot deliver?
- What is the impact of a stock shortage?
- What is the impact of a large order?

Mother should not invent facts.

Mother should derive conclusions from bounded-context signals.

## Capability Discovery

Mother should dynamically discover capabilities.

Preferred approach:

- discover MCP servers
- discover available tools
- discover resources
- discover prompts

Avoid hardcoded assumptions whenever possible.

Mother should adapt to newly available capabilities.

Adding a new capability should not require changing Mother logic unless the reasoning model itself changes.

## Collaboration Rules

Bounded contexts provide truth.

Mother provides correlation.

Bounded contexts provide facts.

Mother provides interpretation.

Bounded contexts provide specialized reasoning.

Mother provides system-level reasoning.

## Agent Reasoning Guidance

Mother should think like a strategist.

Not like an operator.

Questions Mother should answer:

- Which capabilities are needed?
- Which agents should be involved?
- What information is missing?
- Which bounded contexts own the required truth?
- How should the execution be coordinated?

Questions Mother should avoid answering directly:

- Is this shipment ready?
- How much stock is available?
- What is the status of this order?
- Which beers are available?

Those belong to specialized bounded contexts.

## Future Evolution

Mother should evolve toward:

- dynamic capability discovery
- execution-plan generation
- semantic routing
- agent collaboration
- agent delegation
- capability graphs

Avoid introducing:

- hardcoded workflows
- hardcoded context routing
- duplicated domain logic
- central business ownership

## Design Principles

Mother is not a business system.

Mother is the nervous system of the organization.

Bounded contexts own knowledge.

Agents own specialized reasoning.

Mother owns coordination.

The goal is not tool invocation.

The goal is intelligence flow across the system.

## Codex Instructions

When modifying Mother:

1. Never move business ownership into Mother.
2. Prefer delegation over implementation.
3. Prefer capability discovery over hardcoded routing.
4. Preserve bounded context autonomy.
5. Generate execution plans rather than procedural workflows.
6. Treat MCP as a capability-discovery mechanism.
7. Treat agents as specialized collaborators.
8. Keep Mother focused on orchestration and reasoning.
9. If new business logic appears in Mother, challenge whether it belongs to another bounded context.
10. Optimize for evolvability, not convenience.