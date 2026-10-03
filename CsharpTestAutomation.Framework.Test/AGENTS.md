# Framework Self-Test Scope

Read the repository [AGENTS.md](../AGENTS.md) before changing this project. These tests validate reusable framework behavior; do not add application-specific endpoints, DTOs, credentials, or environment assumptions here.

Use NUnit and the existing test utilities. Use WireMock.Net for HTTP behavior and the in-memory SQLite setup for database behavior. Keep tests deterministic and local; do not call external services.

Name test methods `Verify_[ExpectedBehavior]_When_[StateUnderTest]` when the
state adds useful context. Use a shorter `Verify_[ExpectedBehavior]` name when
it stays clear, such as `Verify_DatabaseConnection_StaysOpen`.

Prefer AwesomeAssertions for single and multiple value assertions. Group
independent AwesomeAssertions in `using (new AssertionScope())` so each failure
is reported. Use NUnit's `Assert.EnterMultipleScope()` for multiple NUnit
assertions. Playwright `Expect` failures throw `PlaywrightException` and stop
that scope, so do not use it to collect multiple Playwright failures. If every
independent Playwright check must run, capture each check's failure and report
the failures after all checks complete.

A null-conditional assertion skips the assertion when its target is null. Use
it only when null is an accepted state, for example
`device?.StatusCode.Should().Be(HttpStatusCode.NotFound);`. Assert required
targets are not null before checking their members.

Run the focused test project with:

```pwsh
dotnet test .\CsharpTestAutomation.Framework.Test\CsharpTestAutomation.Framework.Test.csproj
```
