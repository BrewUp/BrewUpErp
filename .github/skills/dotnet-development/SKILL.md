---
name: dotnet-development
description: Guidance for implementing, refactoring, reviewing, and troubleshooting .NET solutions, projects, APIs, workers, libraries, tests, dependency injection, configuration, logging, and packaging. Use when working in a .NET codebase.
license: MIT
---

# .NET development skill

Use this skill when the task involves .NET solution structure, application setup, ASP.NET Core, class libraries, worker services, dependency injection, configuration, logging, data access, testing, packaging, or framework upgrades.

## Goals

- Produce maintainable .NET code that fits the existing repository structure.
- Preserve clear startup, composition, and dependency boundaries.
- Prefer framework conventions over custom infrastructure where conventions already solve the problem.
- Keep generated code testable, observable, and easy to change.

## First, inspect the repository

Before proposing changes:

1. Identify the solution file, project files, target frameworks, test projects, and package management approach.
2. Check whether the repo uses `Directory.Build.props`, `Directory.Packages.props`, analyzers, nullable reference types, implicit usings, global usings, or source generators.
3. Look for architectural conventions already in place for APIs, background services, configuration, persistence, and testing.
4. Reuse existing patterns unless the task explicitly asks to modernize them.

## Solution and project guidance

- Prefer one solution that clearly reflects the application structure.
- Keep project responsibilities focused. Typical project types:
  - web/API host
  - application/service layer
  - domain/core layer
  - infrastructure/adapters
  - shared kernel or contracts only when truly cross-cutting and stable
  - unit and integration test projects
- Avoid creating extra projects unless the separation provides a concrete benefit.
- When adding a project reference, ensure dependencies flow inward toward stable abstractions.

## Framework and language defaults

Unless the repository already follows a different standard:

- Use the repository's current .NET version. If creating new code in a greenfield repo, prefer current LTS unless the user asks otherwise.
- Enable nullable reference types.
- Prefer async APIs for I/O-bound work.
- Use dependency injection instead of service locators or static state.
- Use options binding for configuration.
- Use structured logging.
- Favor SDK-style projects and standard .NET CLI workflows.

## ASP.NET Core guidance

For APIs and web backends:

- Keep `Program.cs` focused on composition and host setup.
- Register services through extension methods when startup grows large.
- Keep controllers or minimal API endpoints thin.
- Place business logic in services, handlers, or use-case classes.
- Validate inputs early and return consistent problem details.
- Prefer built-in authentication and authorization primitives before custom middleware.
- Use cancellation tokens on request-driven async operations.
- Avoid leaking EF Core entities directly from API contracts.

## Configuration and secrets

- Use `appsettings.json` plus environment-specific overrides only for non-secret configuration.
- Bind configuration to strongly typed options classes.
- Validate critical options at startup when possible.
- Never hardcode secrets, connection strings, or tokens.
- Prefer environment variables, user secrets for local development, and platform secret stores in deployed environments.

## Dependency injection

- Register dependencies with the narrowest lifetime that is correct.
- Prefer constructor injection.
- Avoid injecting service providers into application code.
- Avoid large god-services with many dependencies; split by use case or responsibility.
- For service registration, group related registrations in extension methods per area.

## Data access

If the repo uses EF Core:

- Keep DbContext and mapping concerns in infrastructure.
- Model persistence intentionally rather than exposing storage concerns across layers.
- Use migrations consistently with the repository's workflow.
- Avoid N+1 queries and unbounded result sets.
- Project to DTOs or read models when the caller does not need full tracked entities.
- Use transactions deliberately for multi-step writes.

If the repo uses Dapper, ADO.NET, or another persistence approach, follow the established repository conventions rather than mixing patterns casually.

## Logging and observability

- Use `ILogger<T>` and structured log message templates.
- Log enough context to diagnose failures, but do not log secrets or sensitive personal data.
- Prefer correlation-friendly logging in request pipelines and background jobs.
- Add health checks, metrics hooks, or tracing only when aligned with the repo's existing observability stack.

## Error handling

- Fail fast on invalid state and invalid configuration.
- Use domain-specific exceptions sparingly.
- Convert exceptions to user-facing responses at the application edge.
- Do not swallow exceptions silently.
- Include actionable diagnostics in logs.

## Testing expectations

When adding or changing production code:

- Add or update tests for the behavior you changed.
- Prefer unit tests for domain and application logic.
- Use integration tests for infrastructure, HTTP endpoints, and database interactions.
- Keep test names explicit about scenario and expected outcome.
- Avoid brittle tests that depend on incidental implementation details.

## Packaging, build, and maintenance

- Use the repository's package version management style.
- Keep NuGet dependencies minimal and justify new packages.
- Prefer first-party .NET libraries when they satisfy the requirement.
- When upgrading packages or frameworks, note breaking changes and required follow-up work.
- Keep CI-friendly commands simple and scriptable.

## When generating code

Always:

1. Match the repository naming, namespaces, and folder structure.
2. Explain where each new file should live.
3. Include any registrations required for DI, configuration, routing, or middleware.
4. Mention test files that should be added or updated.
5. Highlight assumptions if repository conventions are not visible.

## When reviewing code

Flag issues such as:

- business logic in controllers or `Program.cs`
- missing cancellation tokens on I/O-bound async code
- incorrect service lifetimes
- hidden static mutable state
- unvalidated configuration
- secrets in source control
- EF Core entities leaking across boundaries
- missing tests for behavior changes
- over-engineered abstractions with no clear payoff

## Output style

When responding:

- Prefer repository-specific advice over generic guidance.
- Present changes in implementation order.
- Keep examples idiomatic for modern .NET.
- If multiple approaches are reasonable, recommend one and briefly explain the tradeoff.
