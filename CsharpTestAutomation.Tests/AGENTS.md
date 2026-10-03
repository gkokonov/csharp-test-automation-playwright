# Application Test Scope

Before edits or reviews, read the root [AGENTS.md](../AGENTS.md), shared
[C# rules](../.agents/rules/csharp.md), and [test rules](../.agents/rules/test-automation.md).

| Work | Additional required rules |
| --- | --- |
| API clients, DTOs, builders, fixtures, `ApiTestBase`, or API steps | [API rules](../.agents/rules/api-testing.md). |
| Page objects, components, fixtures, `UiTestBase`, or UI steps | [UI rules](../.agents/rules/ui-testing.md). |
| Supporting helpers or cross-layer scenarios | Load API/UI rules by responsibility; load both when both layers are involved. |
| NetBox Device scenarios or steps | [Device specification](../docs/netbox-automation/03-device-management-spec.md). |

Layer design changes also read the architecture reference linked by the API/UI rule.
Run from the repository root; replace `<Subject>Tests` with the affected fixture:

```pwsh
dotnet build .\CsharpTestAutomation.Tests\CsharpTestAutomation.Tests.csproj
dotnet test .\CsharpTestAutomation.Tests\CsharpTestAutomation.Tests.csproj --filter "FullyQualifiedName~<Subject>Tests"
```

Run live tests only when configured services and credentials are available.
Changed live API/UI fixtures must pass three consecutive headless runs.
For other changes, use the relevant build, local checks, and focused tests.
