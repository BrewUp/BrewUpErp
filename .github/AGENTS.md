# BrewUp ERP

This repository follows:

- Modular Monolith Architecture
- Domain-Driven Design
- CQRS
- Event-Driven integration between modules
- MCP Servers expose module capabilities

Rules:

- Never access another module database directly
- Respect bounded context ownership
- Prefer MCP capabilities over direct integration
- Keep domain language aligned with the bounded context

## Skills

Reusable domain skills live in `.github/skills/` (single source of truth, shared with opencode via
symlinks in `.opencode/skills/*`). Load one with the skill tool when a task matches its description.