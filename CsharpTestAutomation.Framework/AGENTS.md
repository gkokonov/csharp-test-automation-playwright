# Framework Scope

Before edits or reviews, read the root [AGENTS.md](../AGENTS.md), the shared
[C# rules](../.agents/rules/csharp.md), and the canonical
[framework rules](../.agents/rules/framework.md).
Keep application clients, settings, data, and steps in the selected application
test project: `CsharpTestAutomation.Tests` for NUnit or
`CsharpTestAutomation.Bdd.Tests` for BDD. Keep these application layers independent.
`CsharpTestAutomation.Framework.Test` owns local unit/component tests of the framework.

Run from the repository root; replace `<Subject>Tests` with the affected fixture:

```pwsh
dotnet build .\CsharpTestAutomation.Framework\CsharpTestAutomation.Framework.csproj
dotnet test .\CsharpTestAutomation.Framework.Test\CsharpTestAutomation.Framework.Test.csproj --filter "FullyQualifiedName~<Subject>Tests"
```

Run the self-test project without the filter when a change spans fixtures.
