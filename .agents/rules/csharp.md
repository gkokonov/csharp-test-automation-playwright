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
- Prefer primary constructors for new classes and structs when they keep initialization clear; use a regular constructor when its body or validation makes that clearer. Use other modern C# 14 features, including file-scoped namespaces, collection expressions, target-typed `new`, switch expressions, pattern matching, the `field` keyword, and `System.Threading.Lock`, when they improve clarity or reduce boilerplate.
- Keep code concise, idiomatic, object-oriented, functional where useful, maintainable, and testable.
- Prefer simple designs that follow SOLID, DRY, KISS, and YAGNI.
- Use LINQ and lambdas when they improve readability. Use straightforward imperative code when it is clearer.
- Prefer existing repository patterns over a new abstraction unless the change has a concrete need.

## Local Variable Declarations

Follow the three `var` preferences in `.editorconfig`: prefer `var` for built-in
types and when the type is apparent from the right-hand side, but prefer an
explicit type when a non-built-in method call hides the result type. Use `var`
for LINQ query expressions when an anonymous or complex result type makes it
clearer (or necessary); the style analyzer may still suggest an explicit type
where one is available. Do not use `var` for public signatures, fields, or
declarations whose type cannot be inferred.

## Async

- All I/O must be asynchronous, including Playwright, HTTP, database, file, and network operations.
- Use `async`/`await` with `Task` or `Task<T>` and never introduce `.Result`, `.Wait()`, or equivalent sync-over-async code.
- `ScenarioCleanupActions.CleanUp()` may bridge to async only for synchronous callers.
- NUnit synchronous assertion helpers are the documented exception: `.GetAwaiter().GetResult()` may be used only inside those helpers when NUnit requires a synchronous API. Add a short comment that references this NUnit constraint.
- `.ConfigureAwait(false)` is framework-specific. Follow `.agents/rules/framework.md` and `.agents/rules/test-automation.md` for the allowed scope.

## Comments and Documentation

- Prefer self-explanatory code over explanatory comments.
- Add comments only when they explain why something exists, document a constraint or workaround, or clarify non-obvious behaviour.
- Do not narrate straightforward code.
- Public members in `CsharpTestAutomation.Framework` may use XML documentation when it provides useful consumer guidance. Follow the framework rule for required public API documentation.

## Files and Namespaces

- File names should normally match their primary type.
- Namespace structure follows the repository folder structure. Project-specific path and test naming rules belong in the relevant scoped rule file.

## Validation

For C# changes:

1. Run the relevant formatter or analyzer verification.
2. Build the affected project or solution.
3. Run focused tests for the changed scope.
4. Fix diagnostics introduced by the change. Do not suppress them unless the suppression is justified by an existing repository pattern.
