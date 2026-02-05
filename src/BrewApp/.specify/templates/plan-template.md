# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]  
**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]  
**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]  
**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]  
**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]
**Project Type**: [single/web/mobile - determines source structure]  
**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]  
**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]  
**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Review compliance with BrewApp Mobile Constitution principles:

- [ ] **Modular Architecture**: Is this feature scoped as a separate module/project with clear boundaries?
- [ ] **Test-First Development**: Are test scenarios defined and approved before implementation begins?
- [ ] **UX Consistency**: Does design follow established patterns from shared design system?
- [ ] **Code Quality**: Are linting, documentation, and review requirements clear in approach?
- [ ] **Performance Requirements**: Are performance baselines (startup time, frame rate, memory) defined and measurable?

**Violations**: [Document any justified exceptions with rationale and remediation plan]

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., modules/authentication, modules/inventory). The delivered 
  plan must not include Option labels.
  
  MOBILE ARCHITECTURE REQUIREMENT: Per Constitution Principle I (Modular Architecture),
  each module MUST be a separate project with clear boundaries and independent compilation.
-->

```text
# [REMOVE IF UNUSED] Option 1: Mobile Multi-Module (RECOMMENDED for BrewApp per Constitution)
# Each feature is a separate module/project

modules/
├── [FeatureModule1]/               # e.g., modules/Authentication/
│   ├── src/
│   │   ├── models/
│   │   ├── views/
│   │   ├── services/
│   │   └── Module.swift (iOS) or module.gradle (Android)
│   ├── tests/
│   │   ├── unit/
│   │   ├── integration/
│   │   └── ui/
│   └── README.md                   # Module contract and API documentation
├── [FeatureModule2]/
│   └── [same structure]
└── shared/                         # Shared utilities, design system, networking
    ├── ui-components/
    ├── networking/
    └── tests/

app/                                # Main application shell
├── src/
│   ├── AppDelegate/
│   ├── config/
│   └── main/
└── tests/

# [REMOVE IF UNUSED] Option 2: Single project (NOT RECOMMENDED - violates Modular Architecture principle)
src/
├── models/
├── services/
├── views/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 3: Mobile + Backend API
api/                                # Backend services
├── src/
│   ├── models/
│   ├── services/
│   └── endpoints/
└── tests/

modules/                            # Mobile modules (same as Option 1)
└── [as above]
```

**Structure Decision**: [Document the selected structure. For BrewApp, justify if NOT using 
multi-module architecture as it may violate Constitution Principle I unless exception approved]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
