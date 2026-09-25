# Techorama 2026 — Sessionize Submission (Breakout Session)

> Copy-paste fields in the order below. Assumed Sessionize limits: Title 120, Abstract 2000, Outline 5000, Notes 500 characters.

## SESSION TITLE
Domain-Driven Agents: Architecting Enterprise AI with Bounded Contexts

## SESSION ABSTRACT
Creating an AI agent today is easy. Maybe too easy. Most agentic systems are just chatbots wired to a growing pile of tools. It works, until domains, responsibilities and integrations start to grow. Then you hit the same forces software teams have fought for decades: coupling, centralized knowledge, no evolvability. And you end up with a new monolith, masked as intelligence.

There is a better way. We make the case for Domain-Driven Design as the operating system for AI. Bounded contexts were never just about separating software; they are about separating knowledge. When each agent is the custodian of a language, a set of rules and a specific vision of the domain, the system stays coherent, observable and evolvable. MCP turns capabilities into discoverable contracts; A2A lets agents collaborate without centralizing knowledge.

We prove it with real code. BrewUp is an open-source .NET 10 ERP built as a DDD modular monolith with CQRS and event sourcing, shipping a complete AI layer (github.com/BrewUp/BrewUpErp). Live: four MCP servers, one per bounded context; a Knowledge agent speaking A2A; and Mother, a coordination context routing between direct function calling, a deterministic multi-agent workflow and remote agent delegation. Under the hood: Microsoft.Extensions.AI, the MCP SDK, Microsoft.Agents.AI, Azure AI Foundry, semantic chunking and vector search on SQL Server 2025, wrapped in semantic telemetry.

You leave with a concrete blueprint for systems where intelligence flows through the organization the way it should: bounded, discoverable, under control.

## SESSION OUTLINE
1. The problem: why agentic systems become AI monoliths (coupling, centralized knowledge, no evolvability)
2. DDD as the operating system for AI: bounded contexts as knowledge boundaries, agents as custodians of the domain language
3. The platform: BrewUp, a DDD/CQRS/event-sourcing modular monolith in .NET 10 (Docker Compose + Aspire)
4. Capabilities as contracts: four MCP servers, one per bounded context; what a domain-grounded tool looks like
5. Routing intelligence: Mother and its three paths, function calling over pooled MCP tools, a deterministic what-if workflow, A2A delegation
6. Knowledge as a bounded context: semantic chunking, embeddings, vector search on SQL Server 2025; the Knowledge agent
7. Trusted by design: guardrails, semantic telemetry, privacy-safe metrics
8. Takeaways and anti-patterns to avoid

## SESSION LEVEL
Intermediate

## SESSION TYPE
Breakout session (60 minutes)

## TARGET AUDIENCE
.NET developers and software architects starting to build AI features, who want to move from demo chatbots to systems that scale across an organization.

## NOTES TO ORGANIZERS
Open-source repo with full source, live-demoed: https://github.com/BrewUp/BrewUpErp (runs entirely on Docker). The same duo is also available for a full-day workshop (see separate proposal). Speakers: Alberto Acerbis (Microsoft MVP, co-author "Domain-Driven Refactoring", Packt) and Ferdinando Santacroce (socio-technical systems, 25+ years).
