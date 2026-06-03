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