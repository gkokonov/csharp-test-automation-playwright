# Framework Self-Test Scope

Read the repository [AGENTS.md](../AGENTS.md) before changing this project. These tests validate reusable framework behavior; do not add application-specific endpoints, DTOs, credentials, or environment assumptions here.

Use NUnit and the existing test utilities. Use WireMock.Net for HTTP behavior and the in-memory SQLite setup for database behavior. Keep tests deterministic and local; do not call external services.

Run the focused test project with:

```pwsh
dotnet test .\CsharpTestAutomation.Framework.Test\CsharpTestAutomation.Framework.Test.csproj
```
