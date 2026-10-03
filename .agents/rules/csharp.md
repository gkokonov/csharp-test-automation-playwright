---
applyTo: "**/*.cs"
trigger: glob
globs: "**/*.cs"
description: "Repository-wide semantic C# conventions."
---

# C# Development Conventions

Applies to C# source files. Mechanical formatting, naming, using placement, and
analyzer-backed style come from the repository `.editorconfig`.

## Language and Design

- Use the C# language version supported by the repository SDK and target framework.
- Prefer primary constructors when initialization stays clear; use a regular
  constructor when validation or its body makes that clearer. Use modern C#
  features when they improve clarity, subject to `.editorconfig`.
- Keep code concise, idiomatic, object-oriented, functional where useful, maintainable, and testable.
- Prefer simple designs that follow SOLID, DRY, KISS, and YAGNI.
- Use LINQ and lambdas when they improve readability. Use straightforward imperative code when it is clearer.
- Prefer existing repository patterns over a new abstraction unless the change has a concrete need.

## Async

- All I/O must be asynchronous, including Playwright, HTTP, database, file, and network operations.
- Use `async`/`await` with `Task` or `Task<T>` and never introduce `.Result`, `.Wait()`, or equivalent sync-over-async code.
- `ScenarioCleanupActions.CleanUp()` may bridge to async only for synchronous callers.
- NUnit synchronous assertion helpers are the documented exception: `.GetAwaiter().GetResult()` may be used only inside those helpers when NUnit requires a synchronous API. Add a short comment that references this NUnit constraint.
- Require `.ConfigureAwait(false)` only in the framework library; use plain
  `await` in both test projects. Follow the applicable project rules.

## Comments and Documentation

- Prefer self-explanatory code over explanatory comments.
- Add comments only when they explain why something exists, document a constraint or workaround, or clarify non-obvious behaviour.
- Do not narrate straightforward code.
- Keep XML documentation on public framework types and members, as required by
  the [framework rule](framework.md). Document constraints and consumer guidance.

## Files and Namespaces

- File names should normally match their primary type.
- Namespace style follows `.editorconfig`. Project paths and semantic test names
  belong in the relevant scoped rule.

## Validation

For C# changes:

1. Run the relevant formatter or analyzer verification.
2. Build the affected project or solution.
3. Run focused tests for the changed scope.
4. Fix diagnostics introduced by the change. Do not suppress them unless the suppression is justified by an existing repository pattern.
