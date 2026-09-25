# Techorama 2026 — Full-Day Workshop

## Title

**Hands-on Domain-Driven Agents: Build a Multi-Agent System on BrewUp**

## Session format

- Type: Full-day workshop (October 26, 2026)
- Level: Intermediate
- Track: AI / Architecture / Modern .NET
- Speakers: Alberto Acerbis + Ferdinando Santacroce (co-presentation)

## Abstract

Stop talking about agents — build one. This is a full-day, hands-on workshop on architecting agentic AI systems with Domain-Driven Design, built on BrewUp, an open-source .NET 10 ERP (DDD, CQRS, event sourcing) that already ships a complete AI layer.

You'll get your hands on real code, not slides. Starting from a modular monolith, we'll explore how bounded contexts become knowledge boundaries for AI: each agent is the custodian of a language, a set of rules and a specific vision of the domain. Throughout the day you will:

- Run the four MCP servers (one per bounded context) and consume their tools from the "Mother" coordination layer and from external MCP clients
- Extend the platform: add a new domain tool to a bounded-context MCP server, with unit tests
- Build a knowledge pipeline: semantic chunking, embeddings and vector search on SQL Server 2025
- Wire the three routing paths of the coordinator: direct function calling over pooled MCP tools, a deterministic multi-agent workflow, and A2A delegation to a remote agent
- Add semantic telemetry and guardrails to make the system observable and safe by design

We'll use Microsoft.Extensions.AI, the MCP SDK, Microsoft.Agents.AI, Azure AI Foundry and .NET 10. Everything runs in Docker; an OpenAI-compatible endpoint is a plus, but the workshop is designed to work offline with fake embeddings and local models.

Who is it for: .NET developers and architects comfortable with C# who want to move from chatbots to real agentic systems. Bring a laptop with the .NET 10 SDK and Docker installed.

## Outline

1. **09:00 — Why agentic systems become AI monoliths.** The DDD operating-system thesis: coupling, centralized knowledge, evolvability.
2. **09:30 — Standing up BrewUp.** Docker Compose, Aspire, EventStoreDB, MongoDB, RabbitMQ, SQL Server.
3. **10:00 — Tour of the four MCP servers.** How bounded contexts map to capability contracts; consuming tools from VS Code / Claude Code.
4. **11:00 — Lab 1: extend a bounded context.** Add a new tool to an MCP server + unit tests.
5. **12:00 — Lunch.**
6. **13:00 — The Knowledge bounded context.** Semantic chunking, embeddings, SQL Server 2025 vector search. **Lab 2:** ingest a document and search it.
7. **14:15 — Mother: routing intelligence.** The three paths (function calling, deterministic workflow, A2A delegation). **Lab 3:** route a "what-if" question through the agent chain.
8. **15:15 — Trusted by design.** Guardrails and semantic telemetry. **Lab 4:** observe an agent run end to end.
9. **16:15 — Open lab + recap.** Taking the patterns home; anti-patterns to avoid.
10. **17:00 — End.**

## Takeaways

- A mental model for applying DDD to AI systems: bounded contexts as knowledge boundaries
- A working local agentic platform to keep and extend after the workshop
- Hands-on familiarity with the MCP SDK, Microsoft.Extensions.AI, Microsoft.Agents.AI and vector search on SQL Server 2025
- A blueprint for routing, guardrails and observability in multi-agent systems

## Prerequisites

- Laptop with the .NET 10 SDK and Docker installed (before the day, we share a `docker compose up` checklist)
- Recommended (not required): an Azure AI Foundry or OpenAI-compatible endpoint; the workshop degrades gracefully to offline mode
- Git clone of **https://github.com/BrewUp/BrewUpErp**

## Notes to the selection committee

- Public repository with the full source: **https://github.com/BrewUp/BrewUpErp** — every lab is built on real, open-source code
- The workshop runs entirely on Docker; offline fallbacks (fake embeddings, local models) make it robust in any venue
- The same duo also submits a 60-minute breakout session on the same topic (see the separate submission); the workshop is a natural deep-dive companion
- Speakers: Alberto Acerbis (Microsoft MVP .NET, co-author of "Domain-Driven Refactoring", Packt) and Ferdinando Santacroce (socio-technical systems, 25+ years across 80+ teams and 20+ companies)
- The brewery theme of BrewUp adapts nicely to the Medieval Edition; we can tailor examples to the conference theme
