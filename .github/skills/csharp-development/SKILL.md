---
name: csharp-development
description: Guidance for writing, reviewing, and refactoring idiomatic C# with strong naming, null-safety, async correctness, collections, LINQ, error handling, and maintainability. Use when working directly with C# code.
license: MIT
---

# C# development skill

Use this skill for tasks focused on C# implementation quality: class design, methods, records, pattern matching, async code, LINQ, exceptions, nullability, performance-sensitive paths, and refactoring.

## Core principles

- Prefer clarity over cleverness.
- Match the repository's style first; otherwise use modern idiomatic C#.
- Keep types small, cohesive, and easy to understand.
- Use the type system to make invalid states harder to represent.
- Optimize only where measurement or scale justifies it.

## Naming and API design

- Choose names that reflect intent, not implementation mechanics.
- Public members should read naturally and be unsurprising.
- Use nouns for types and verbs for methods.
- Avoid ambiguous utility names like `Helper`, `Manager`, `Processor`, or `Common` unless the repository already uses them intentionally.
- Prefer explicit method names over comments that explain vague names.

## Type design

- Prefer `record` or `record struct` for immutable value-like data when appropriate.
- Prefer classes for entities with identity, lifecycle, or mutation.
- Keep mutable state localized and controlled.
- Expose the smallest useful surface area.
- Prefer composition over inheritance unless inheritance models a real is-a relationship and remains easy to reason about.

## Nullability

- Respect nullable reference types.
- Avoid `!` unless you can justify it confidently.
- Model optional values explicitly.
- Validate method inputs at boundaries.
- Return empty collections instead of `null` collections.

## Methods and control flow

- Keep methods focused on one purpose.
- Prefer early returns to deeply nested conditionals.
- Use guard clauses for invalid inputs or unsupported states.
- Break up long methods when they mix orchestration with detailed logic.
- Use local functions only when they improve readability.

## Async and concurrency

- Use `async` and `await` for I/O-bound work.
- Do not block on async code with `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` unless absolutely necessary and safe.
- Propagate `CancellationToken` when available and relevant.
- Avoid `async void` except for event handlers.
- Use `ValueTask` only when there is a clear benefit and the repository already uses it correctly.

## Collections and LINQ

- Prefer the least powerful abstraction that fits the need.
- Use `IReadOnlyCollection<T>` or `IReadOnlyList<T>` for read-only public contracts when ordering/count matters.
- Avoid multiple enumeration of expensive sequences.
- Keep LINQ readable; do not compress complex business rules into one query chain.
- Materialize intentionally when deferred execution could surprise readers or duplicate work.

## Exceptions and validation

- Use exceptions for exceptional situations, not routine control flow.
- Throw specific exception types when helpful.
- Validate external input at the boundary.
- Include useful messages, but do not leak secrets or sensitive data.
- Prefer result types or domain responses when failure is expected and part of normal business flow.

## Pattern matching and modern C#

Use modern language features when they improve readability:

- pattern matching for shape checks and branching
- switch expressions for clear value mapping
- primary constructors only when they improve the design and match repository conventions
- collection expressions and target-typed new where they make code simpler

Do not force new syntax into code if it makes the result less familiar for the team.

## Immutability and side effects

- Prefer immutable DTOs and message types.
- Isolate side effects near boundaries such as databases, file systems, HTTP calls, and message buses.
- Keep pure transformation logic separate from side-effecting orchestration where practical.

## Performance guidance

Only optimize after identifying a real hotspot or when writing obviously high-volume code.

Watch for:

- repeated allocations in tight loops
- unnecessary string concatenation in hot paths
- hidden multiple enumeration
- inefficient LINQ on large collections
- avoidable boxing
- accidental synchronous waits around I/O

Prefer maintainable code unless performance requirements clearly justify extra complexity.

## Refactoring guidelines

When refactoring:

1. Preserve behavior unless the task explicitly includes behavioral changes.
2. Improve names before introducing new abstractions.
3. Remove duplication after confirming the duplicated logic is truly the same.
4. Add or update tests around the change.
5. Avoid broad rewrites when a focused improvement solves the problem.

## Review checklist

Flag issues such as:

- vague names
- long methods with mixed responsibilities
- hidden null risks
- sync-over-async patterns
- magic strings or unexplained constants
- broad catch blocks
- premature abstraction
- complex LINQ that obscures business rules
- mutable shared state without clear synchronization

## Generation guidelines

When generating C# code:

- Match existing namespace and file layout conventions.
- Include required `using` directives only if needed.
- Prefer concise, idiomatic code over framework-heavy ceremony.
- Explain any non-obvious tradeoffs.
- Add tests when new behavior is introduced.

## Output style

- Show the simplest correct implementation first.
- Prefer small, composable examples.
- When there are alternatives, recommend one and briefly say why.
