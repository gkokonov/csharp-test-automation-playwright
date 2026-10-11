# Framework review checklist

Read CsharpTestAutomation.Framework/AGENTS.md and its canonical C#/framework rules.
Check reusable boundaries and configuration through CoreConfiguration, public
contract compatibility, dependency/lifetime ownership, cancellation and async
behavior, safe retry semantics, optional diagnostics overhead, failure evidence
before release, complete teardown and preservation of primary failures.
Inspect framework self-tests for the affected behavior. Do not add application
endpoints, DTOs, or feature-specific policy to the packable framework.
Apply .agents/review/REVIEW.md for evidence, scope, and reporting.
