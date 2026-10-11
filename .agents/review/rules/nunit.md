# NUnit application review checklist

Read CsharpTestAutomation.Tests/AGENTS.md and applicable C#, shared test, API/UI
rules, layer architecture, and the affected feature specification.
Check native response/serialization contracts, client/data ownership, meaningful
assertions, test isolation and parallel safety, fixture lifecycle and cleanup.
For UI check locator/readiness behavior, page-object responsibilities, browser
lifetime, optional diagnostics and trace collection before disposal.
Check live-service prerequisites and validation claims; preserve explicit accepted
exceptions. Apply .agents/review/REVIEW.md for evidence, scope, and reporting.
