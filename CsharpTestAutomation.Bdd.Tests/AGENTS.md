# BDD Application Test Scope

Before edits or reviews, read the root [AGENTS.md](../AGENTS.md), shared
[C# rules](../.agents/rules/csharp.md), [test rules](../.agents/rules/test-automation.md),
and [BDD rules](../.agents/rules/bdd-testing.md).

| Work | Additional required guidance |
| --- | --- |
| Clients, DTOs, builders, or API step definitions | [API rules](../.agents/rules/api-testing.md). |
| Pages, components, or UI step definitions | [UI rules](../.agents/rules/ui-testing.md). |
| Hooks, lifecycle, authentication, or supporting helpers | Load API/UI rules by responsibility; load both for cross-layer work. |
| NetBox Device features, bindings, or data | [Device specification](../docs/netbox-automation/03-device-management-spec.md). |

Layer design changes also read the architecture reference linked by the API/UI
rule. Application layers remain in this project; do not reference the NUnit
application assembly. Step definitions live in `Steps/API/<App>/` or
`Steps/UI/<App>/` and use the `Steps` class suffix. They include scenario setup,
actions, assertions, and cleanup registration without a separate workflow layer.
Features and step definitions use Reqnroll hooks instead of
NUnit fixture bases.

Run from the repository root:

```pwsh
dotnet build .\CsharpTestAutomation.Bdd.Tests\CsharpTestAutomation.Bdd.Tests.csproj
dotnet test .\CsharpTestAutomation.Bdd.Tests\CsharpTestAutomation.Bdd.Tests.csproj --filter "TestCategory=SiteApi01"
dotnet test .\CsharpTestAutomation.Bdd.Tests\CsharpTestAutomation.Bdd.Tests.csproj --filter "TestCategory=Device"
```

Run live scenarios only when services and credentials are configured. Changed
live features or bindings must pass three consecutive headless runs. For other
changes, use relevant local checks and focused tests. For instruction-only
changes, use the [maintenance check](../docs/AGENT_SUPPORT.md#instruction-maintenance-check).
