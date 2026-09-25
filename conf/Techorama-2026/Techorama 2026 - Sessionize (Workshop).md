# Techorama 2026 — Sessionize Submission (Full-Day Workshop)

> Copy-paste fields in the order below. Assumed Sessionize limits: Title 120, Abstract 2000, Outline 5000, Notes 500 characters.

## SESSION TITLE
Hands-on Domain-Driven Agents: Build a Multi-Agent System on BrewUp

## SESSION ABSTRACT
Stop talking about agents: build one. A full-day, hands-on workshop on architecting agentic AI systems with Domain-Driven Design, built on BrewUp, an open-source .NET 10 ERP (DDD, CQRS, event sourcing) that already ships a complete AI layer.

You will work on real code, not slides. Starting from a modular monolith, we explore how bounded contexts become knowledge boundaries for AI: each agent is the custodian of a language, a set of rules and a vision of the domain. During the day you will:
- Run the four MCP servers (one per bounded context) and consume their tools from the Mother coordination layer and external MCP clients
- Extend the platform: add a new domain tool to an MCP server, with unit tests
- Build a knowledge pipeline: semantic chunking, embeddings, vector search on SQL Server 2025
- Wire the three routing paths of the coordinator: direct function calling over pooled MCP tools, a deterministic multi-agent workflow, and A2A delegation to a remote agent
- Add semantic telemetry and guardrails to make the system observable and safe by design

Stack: Microsoft.Extensions.AI, the MCP SDK, Microsoft.Agents.AI, Azure AI Foundry, .NET 10. Everything runs on Docker; an OpenAI-compatible endpoint is a plus, but the workshop degrades gracefully to offline mode (fake embeddings, local models).

For: .NET developers and architects comfortable with C# who want to move from chatbots to real agentic systems. Bring a laptop with the .NET 10 SDK and Docker installed.

## SESSION OUTLINE
1. 09:00 Why agentic systems become AI monoliths; the DDD operating-system thesis
2. 09:30 Standing up BrewUp: Docker Compose, Aspire, EventStoreDB, MongoDB, RabbitMQ, SQL Server
3. 10:00 Tour of the four MCP servers: bounded contexts as capability contracts; consume them from VS Code and Claude Code
4. 11:00 Lab 1: add a new tool to a bounded-context MCP server, with unit tests
5. 12:00 Lunch
6. 13:00 The Knowledge bounded context: semantic chunking, embeddings, SQL Server 2025 vector search. Lab 2: ingest a document and search it
7. 14:15 Mother: the three routing paths. Lab 3: route a what-if question through the agent chain
8. 15:15 Trusted by design: guardrails and semantic telemetry. Lab 4: observe an agent run end to end
9. 16:15 Open lab and recap: patterns to take home, anti-patterns to avoid
10. 17:00 End

## SESSION LEVEL
Intermediate

## SESSION TYPE
Full-day workshop

## PREREQUISITES
Laptop with .NET 10 SDK and Docker installed (pre-event setup checklist shared). Recommended, not required: an Azure AI Foundry or OpenAI-compatible endpoint. Clone: https://github.com/BrewUp/BrewUpErp

## NOTES TO ORGANIZERS
All labs run on real open-source code: https://github.com/BrewUp/BrewUpErp. Fully Docker-based with offline fallbacks. The same duo also submits a 60-minute breakout on the same topic (see separate proposal). Speakers: Alberto Acerbis (Microsoft MVP, co-author "Domain-Driven Refactoring", Packt) and Ferdinando Santacroce (socio-technical systems, 25+ years).
