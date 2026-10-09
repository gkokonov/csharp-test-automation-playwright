---
applyTo: "CsharpTestAutomation.Tests/API/**/*.cs,CsharpTestAutomation.Tests/Tests/API/**/*.cs,CsharpTestAutomation.Tests/Tests/ApiTestBase.cs,CsharpTestAutomation.Tests/Steps/API/**/*.cs,CsharpTestAutomation.Tests/TestData/API/**/*.cs,CsharpTestAutomation.Bdd.Tests/API/**/*.cs,CsharpTestAutomation.Bdd.Tests/Steps/API/**/*.cs,CsharpTestAutomation.Bdd.Tests/TestData/API/**/*.cs,CsharpTestAutomation.Bdd.Tests/Features/*Api.feature"
trigger: glob
globs: "CsharpTestAutomation.Tests/API/**/*.cs, CsharpTestAutomation.Tests/Tests/API/**/*.cs, CsharpTestAutomation.Tests/Tests/ApiTestBase.cs, CsharpTestAutomation.Tests/Steps/API/**/*.cs, CsharpTestAutomation.Tests/TestData/API/**/*.cs, CsharpTestAutomation.Bdd.Tests/API/**/*.cs, CsharpTestAutomation.Bdd.Tests/Steps/API/**/*.cs, CsharpTestAutomation.Bdd.Tests/TestData/API/**/*.cs, CsharpTestAutomation.Bdd.Tests/Features/*Api.feature"
description: "Application API clients, DTOs, builders, fixtures, API steps, and API helpers, including setup used by UI tests."
---

# Application API Rules

Read the [shared test rules](test-automation.md). For API layer design or new
clients/DTOs, read [API architecture](../../docs/API_TESTING_ARCHITECTURE.md).
Helpers follow these rules when their responsibility involves API behavior.

## Clients, DTOs, and builders

- `API/Clients/`: Typed clients under the selected project's `Api.Clients` namespace.
- `API/DTOs/<Resource>/`: Request/response DTOs in `Api.Dtos.<Resource>`.
- `API/Factories/`: NBuilder-backed request builders, such as
  `CreateResourceDtoBuilder : BaseBuilder<CreateResourceDto>`, in `Api.Factories`.
- Keep upper-case `API/DTOs/` paths and Pascal-case `Api.Dtos` namespaces.
  Likewise, `Steps/API/` maps to `Steps.Api`. These are intentional casing choices.
- Build request payloads with these builders rather than inline DTO construction.
  Shared static values belong in `TestData/API/`.
- Clients own their endpoint requests and return native RestSharp responses;
  do not add a framework request/execution/response abstraction.

## NUnit fixtures and lifetime

In `CsharpTestAutomation.Tests`, every API fixture derives from `ApiTestBase`,
even without DB prerequisites. BDD setup and lifetime follow [BDD rules](bdd-testing.md).
Do not repeat inherited `[TestFixture]`, `[AllureNUnit]`, or `[Category("API")]`.

Construct and register clients in `OnSetUpAsync()` after its base call, using
the inherited `RestClientFactory` and configured authenticator. For NetBox,
use the inherited `NetBoxAuthenticator`:

```csharp
private FooApiClient FooClient => GetClient<FooApiClient>();

protected override async Task OnSetUpAsync()
{
    await base.OnSetUpAsync();
    RegisterClient(new FooApiClient(RestClientFactory, NetBoxAuthenticator));
}
```

- Use `RegisterClient`/`GetClient<T>`; fixtures never access `TestContainer` directly.
  Do not hold owned disposable clients in plain fixture fields.
- `TestBase` disposes registered clients after `ScenarioCleanupActions.CleanUpAsync()`;
  clients must remain usable during cleanup. Do not suppress `NUnit1032` to bypass this.
- Use the inherited cleanup stack, including in borrowed API steps.

## Prerequisites

In NUnit fixtures, use `ApiTestBase.RequireDbData<T>` for nullable/empty environment seed data.
Missing seed data makes the test inconclusive; do not create unrelated fallback
data. Do not use this helper for a record just created by the test: a missing
created record is a failed assertion. Follow shared ownership/cleanup rules.

## Assertions and diagnostics

- Use AwesomeAssertions for status codes, headers, content type, and deserialized
  payloads. Use `ApiAssertions` only for transport completion
  (`ShouldHaveCompletedTransport`) and JSONPath (`ShouldHaveJsonPathValue`).
- Assert critical status codes before extracting data or performing DB operations.
  Apply the shared contract-based DB policy; fixtures/Then bindings own behavior assertions.
- Keep request/response redaction enabled for normal NetBox runs so expected
  `404` responses do not retain raw Authorization headers in Allure attachments.
  Startup configuration logging currently writes unredacted values; changing
  that behavior is deferred until requested.
