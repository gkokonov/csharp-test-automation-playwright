# Framework self-test review checklist

Read CsharpTestAutomation.Framework.Test/AGENTS.md and canonical C# rules.
Check deterministic NUnit tests using local WireMock/SQLite or equivalent existing
patterns, plain await, useful assertions, isolated state, resource cleanup,
failure paths, and framework contract coverage. External application endpoints,
credentials, or services must not be prerequisites.
Apply .agents/review/REVIEW.md for evidence, scope, and reporting.
