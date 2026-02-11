<!--
Sync Impact Report
- Version change: (template) → 1.0.0
- Modified principles:
	- [PRINCIPLE_1_NAME] → I. Maintainable, Modular Architecture
	- [PRINCIPLE_2_NAME] → II. Code Quality & Readability
	- [PRINCIPLE_3_NAME] → III. Testing Discipline (NON-NEGOTIABLE)
	- [PRINCIPLE_4_NAME] → IV. Consistent User Experience
	- [PRINCIPLE_5_NAME] → V. Performance & Observability
- Added sections:
	- Non-Functional Standards & Constraints
	- Development Workflow & Quality Gates
- Removed sections:
	- None
- Templates:
	- ✅ .specify/templates/plan-template.md (Constitution Check must enforce principles 1–5)
	- ✅ .specify/templates/spec-template.md (User scenarios and requirements feed UX, testing, and performance)
	- ✅ .specify/templates/tasks-template.md (Tasks organized for independently testable, modular slices)
	- ✅ .specify/templates/agent-file-template.md (Generic, no constitution-specific changes required)
	- ✅ .specify/templates/checklist-template.md (Generic, no constitution-specific changes required)
- Follow-up TODOs:
	- None
-->

# BrewApp ERP Constitution

## Core Principles

### I. Maintainable, Modular Architecture

Architecture MUST favour small, cohesive modules with clear responsibilities and
well-defined boundaries. Shared logic MUST live in dedicated, reusable modules
instead of being copied across the codebase. Dependencies MUST flow in a
consistent direction (e.g., domain → application → infrastructure) with no
cyclic imports between modules or layers.

- Every module MUST expose a documented public API (types, interfaces,
	endpoints, events) and hide internal details.
- Breaking changes to a public API MUST include a migration note and be
	captured in the plan and tasks for the feature introducing the change.
- New features MUST integrate by extending existing modules or adding new
	modules, not by bypassing boundaries (e.g., direct database access from UI).
- The structure described in feature plans MUST match the real directories so
	new developers can navigate by feature/module.

Rationale: A modular architecture with explicit contracts makes the system
easier to understand, test, and evolve for both existing and new team members.

### II. Code Quality & Readability

Code MUST prioritize clarity and correctness over cleverness. Naming, formatting
and patterns MUST be consistent across the project so that any developer can
read and reason about unfamiliar code quickly.

- Automated formatting and linting MUST be enabled and kept passing on every
	branch before merging.
- Code reviews MUST check for readability, dead code, duplication, and
	adherence to agreed patterns (e.g., service-layer abstractions, error
	handling, logging).
- Public functions, complex flows, and non-trivial business rules MUST be
	documented inline or in module-level documentation (e.g., README, diagrams).
- New contributors SHOULD be able to understand the purpose and usage of a
	module within one focused session (≤ 1 hour) without relying on tribal
	knowledge.

Rationale: Consistent, readable code dramatically lowers onboarding time and
reduces the risk of regressions when the team changes or grows.

### III. Testing Discipline (NON-NEGOTIABLE)

Automated tests are mandatory for all behaviour that can be reasonably tested.
No change introducing or modifying behaviour MAY be merged without appropriate
test coverage.

- New features MUST include tests at the right level (unit, integration,
	contract, or end-to-end) as part of the same change.
- Bug fixes MUST include a regression test that fails before the fix and
	passes after it.
- Critical business flows and core modules MUST have integration or contract
	tests that validate end-to-end behaviour, not just isolated units.
- The default approach SHOULD follow a test pyramid: many fast unit tests,
	fewer integration tests, and a small number of end-to-end flows.
- All tests MUST run as part of the CI pipeline; a failing test MUST block the
	merge.

Rationale: Strong testing discipline is essential to keep the product stable,
support refactoring, and give new developers confidence when making changes.

### IV. Consistent User Experience

User-facing behaviour MUST be consistent across the product. Screens, flows,
and interactions that represent the same concept MUST use the same terminology,
components, and visual patterns.

- Shared UI components, design tokens, and patterns MUST be reused rather than
	re-implemented per feature.
- Navigation and error handling MUST behave consistently for similar flows
	(e.g., form submission, confirmation dialogs, validation messages).
- New user stories in specifications MUST include acceptance criteria that
	describe the expected user experience, not only backend behaviour.
- UX changes that could surprise existing users MUST be called out in the plan
	and reviewed explicitly (e.g., changed workflows, removed options).
- Accessibility and responsiveness SHOULD be considered for all new UI work and
	MUST be addressed for critical user journeys.

Rationale: A consistent experience builds user trust, reduces support burden,
and makes it easier for developers to extend existing flows without breaking
expectations.

### V. Performance & Observability

Performance is treated as a first-class feature. Behaviour MUST remain within
acceptable performance thresholds, and the system MUST be observable in
production.

- Feature plans MUST state performance goals and constraints (latency,
	throughput, memory, data volume). "NEEDS CLARIFICATION" is not acceptable
	once implementation begins.
- Known performance budgets (e.g., page load times, API p95 latency) MUST be
	respected; regressions discovered in tests or production are treated as
	defects.
- Critical paths (e.g., main dashboards, order flows, financial operations)
	MUST have basic profiling or benchmarking at least once per major change.
- Logging, metrics, and (where applicable) tracing MUST provide enough
	information to debug issues without adding ad-hoc print statements.
- Expensive operations (e.g., data imports, batch jobs) MUST be designed with
	back-pressure, pagination, or batching to avoid impacting interactive users.

Rationale: Observable, performant software keeps the ERP responsive at scale
and makes it feasible to diagnose issues quickly.

## Non-Functional Standards & Constraints

This section translates the core principles into concrete, cross-cutting
standards that apply to every feature and change.

- **Languages and tooling**: Each active language MUST have an agreed formatter
	and linter configured (e.g., via project configuration files). CI MUST run
	these tools and block merges on violations.
- **Module documentation**: Every non-trivial module or bounded context MUST
	include a short README (or equivalent documentation) explaining its purpose,
	main entry points, dependencies, and any non-obvious decisions.
- **Dependency rules**: Business logic MUST live in domain or application
	layers, not in controllers, UI components, or persistence adapters.
	Infrastructure concerns (database, HTTP, messaging) MUST depend on domain
	contracts, not the other way around.
- **Backward compatibility**: Any change that may break existing consumers
	(modules, APIs, jobs, UI flows) MUST be identified in the plan, require
	reviewer approval, and include a migration or fallback strategy.
- **Performance definitions**: The "Performance Goals" and "Constraints"
	fields in implementation plans MUST be filled with concrete, measurable
	targets before coding starts. Leaving them vague or unfilled is a
	constitution violation.
- **Onboarding support**: Each feature MUST update or create minimal
	documentation (e.g., quickstart steps, data model notes) so a new developer
	can:
	- Run the relevant tests locally with a single documented command, and
	- Understand where to place related code for future changes.

## Development Workflow & Quality Gates

The development workflow operationalizes the constitution into day-to-day
practice.

- **Spec and plan first**: Every meaningful feature MUST start from a
	specification and an implementation plan. User stories and acceptance
	criteria drive tests; the plan describes architecture, performance, and
	dependencies.
- **Constitution Check**: The "Constitution Check" section of implementation
	plans MUST explicitly describe how the feature complies with principles 1–5
	and list any exceptions. Exceptions MUST be justified and recorded in the
	"Complexity Tracking" table.
- **Quality gates for merge**:
	- All relevant tests MUST pass in CI.
	- Linters/formatters MUST pass with no new warnings/errors.
	- For user-facing changes, at least one reviewer MUST confirm UX consistency
		with existing flows.
	- For performance-sensitive paths, reviewers MUST verify that performance
		goals are realistic and not obviously violated.
- **Incremental delivery**: User stories MUST be independently implementable
	and testable. It MUST be possible to ship a story as an MVP without
	partially implemented flows leaking into production.
- **Refactoring safety**: Refactors that do not change behaviour MUST keep test
	coverage intact or improved. Large refactors SHOULD be split into safe,
	reviewable increments.

## Governance

This constitution governs how the BrewApp ERP project is built and evolved. It
supersedes ad-hoc practices and undocumented conventions.

- **Authority and precedence**: When in conflict, this constitution overrides
	local or historical habits. Code reviews MUST enforce these principles.
- **Amendment process**:
	- Any team member MAY propose amendments by documenting the change,
		rationale, and impact on existing principles.
	- Amendments MUST be reviewed and approved by the maintainers or designated
		technical leads.
	- Approved amendments MUST update this file, including the Sync Impact
		Report and version metadata.
- **Versioning policy**:
	- MAJOR version (X.0.0): Backward-incompatible governance changes (e.g.,
		removing or fundamentally redefining a principle).
	- MINOR version (x.Y.0): Adding a new principle or materially expanding
		guidance that affects how work is done.
	- PATCH version (x.y.Z): Clarifications, wording fixes, and non-semantic
		refinements.
- **Review cadence**: The team SHOULD review this constitution at regular
	intervals (e.g., quarterly) and after major architectural shifts.
- **Compliance expectation**: All new work, including spikes and experiments,
	MUST respect these principles unless explicitly documented and approved as a
	temporary exception.

**Version**: 1.0.0 | **Ratified**: 2026-02-11 | **Last Amended**: 2026-02-11
