# Techorama 2026 — Breakout Session (60 min)

## Title

**Domain-Driven Agents: Architecting Enterprise AI with Bounded Contexts**

## Session format

- Type: Breakout session (60 minutes)
- Level: Intermediate
- Track: AI / Architecture (fits both the "Trusted AI" and "Modern .NET" learning journeys)
- Speakers: Alberto Acerbis + Ferdinando Santacroce (co-presentation)

## Abstract

Creating an AI agent today is easy. Maybe too easy. Most agentic systems are just chatbots wired to a growing pile of tools. It works — until domains, responsibilities and integrations start to grow. Then you hit the same forces we've been fighting in software for decades: coupling, centralized knowledge, no evolvability. And you end up with a new monolith, masked as intelligence.

There's a better way. In this session we make the case for Domain-Driven Design as the operating system for AI. Bounded contexts were never just about separating software — they are about separating knowledge. When each agent is the custodian of a language, a set of rules and a specific vision of the domain, the whole system stays coherent, observable and evolvable. MCP turns those capabilities into discoverable contracts. A2A lets agents collaborate without centralizing knowledge.

We'll prove it with real code. BrewUp is an open-source .NET 10 ERP built as a DDD modular monolith with CQRS and event sourcing — and it ships a complete, production-shaped AI layer. You'll see live: four MCP servers, one per bounded context; a Knowledge agent speaking A2A; and "Mother", a coordination context that routes between direct function calling, a deterministic multi-agent workflow and remote agent delegation. Under the hood: Microsoft.Extensions.AI, the MCP SDK, Microsoft.Agents.AI, Azure AI Foundry, semantic chunking and vector search on SQL Server 2025 — all wrapped in semantic telemetry.

You'll leave with a concrete blueprint for designing systems where intelligence flows through the organization the way it should: bounded, discoverable and under control.

## Outline

1. The problem: why agentic systems become AI monoliths (coupling, centralized knowledge, no evolvability)
2. DDD as the operating system for AI: bounded contexts as knowledge boundaries, agents as custodians of the domain language
3. The platform: BrewUp, a DDD/CQRS/event-sourcing modular monolith in .NET 10
4. Capabilities as contracts: four MCP servers, one per bounded context — what a domain-grounded tool looks like
5. Routing intelligence: "Mother" and its three paths (function calling over pooled MCP tools, deterministic what-if workflow, A2A delegation)
6. Knowledge as a bounded context: semantic chunking → embeddings → vector search on SQL Server 2025, and the Knowledge agent
7. Trusted by design: guardrails, semantic telemetry and privacy-safe metrics
8. Takeaways and anti-patterns to avoid

## What attendees will learn

- Why coupling and centralized knowledge turn agent systems into AI monoliths — and how bounded contexts prevent it
- How to expose a bounded context as a discoverable, versionable set of MCP tools
- How to design routing between direct function calling, deterministic workflows and A2A delegation
- How to build a knowledge context with semantic chunking and vector search on SQL Server 2025
- How to keep agentic systems observable with semantic spans and privacy-safe metrics

## Target audience

.NET developers and software architects working on (or about to start) AI features, who want to move from demo chatbots to systems that scale across an organization.

## Notes to the selection committee

- Public repository with the full source, live-demoed during the session: **https://github.com/BrewUp/BrewUpErp**
- Everything shown is open source and runs entirely on Docker — no fake slides, the demo is the real code
- The same duo is also available to run a full-day hands-on workshop on the same topic (see the separate workshop submission) if the organizers find it valuable
- Speakers: Alberto Acerbis (Microsoft MVP .NET, co-author of "Domain-Driven Refactoring", Packt) and Ferdinando Santacroce (socio-technical systems, 25+ years across 80+ teams and 20+ companies)
- The brewery theme of BrewUp adapts nicely to the Medieval Edition; we can tailor visuals and examples to the conference theme
