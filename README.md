# C# Test Automation Playwright

Reusable .NET test infrastructure and application tests for API, database, and browser workflows. The solution targets .NET 10 and uses NUnit, Microsoft.Playwright, RestSharp, Dapper, and Allure.

## Projects

| Project | Purpose |
| --- | --- |
| `CsharpTestAutomation.Framework` | Reusable, packable test infrastructure. |
| `CsharpTestAutomation.Framework.Test` | Unit and local integration tests for the framework. |
| `CsharpTestAutomation.Tests` | Application API and UI clients, test data, and tests. Requires configured access to the application, identity provider, and database. |

## Build and Test

Run from the repository root in PowerShell:

```pwsh
dotnet restore
dotnet build .\CsharpTestAutomation.slnx
dotnet test .\CsharpTestAutomation.Framework.Test\CsharpTestAutomation.Framework.Test.csproj
```

Application tests use settings in `CsharpTestAutomation.Tests/appsettings.json`. Do not put credentials in tracked files. When `Environment` is not set, `appsettings.local.json` can override local settings and is ignored by Git. When `Environment` is set, the matching `appsettings.<Environment>.json` file is used instead. Configure valid identity-provider and database credentials before running application tests; the checked-in placeholders are not usable credentials.

The application test default is headless. To debug with a visible browser, set `HeadlessMode` to `false` in the ignored local settings file.

## Playwright Browser

Build the application test project, then install the configured Chromium browser:

```pwsh
dotnet build .\CsharpTestAutomation.Tests\CsharpTestAutomation.Tests.csproj
pwsh .\CsharpTestAutomation.Tests\bin\Debug\net10.0\playwright.ps1 install chromium
```

Run a focused application fixture only when its external services and credentials are available:

```pwsh
dotnet test .\CsharpTestAutomation.Tests\CsharpTestAutomation.Tests.csproj --filter "FullyQualifiedName~<FixtureName>"
```

## Agent Guidance

See [AGENTS.md](AGENTS.md) for task routing and [agent support](docs/AGENT_SUPPORT.md)
for Copilot, Codex, and Antigravity discovery. The root targets 60–80 lines with
a 100-line maximum; project entry points load shared and applicable layer rules.
Set `CONTEXT7_API_KEY` in the local agent environment for the optional Context7
MCP server; do not store the key in the repository.

For local Windows Codex clients, open and trust this repository, then start a fresh session. Codex uses the root and scoped `AGENTS.md` files, `.agents/skills/`, and the project MCP settings in `.codex/config.toml`. VS Code's `.vscode/mcp.json` remains the Copilot adapter. See [Windows Codex setup and checks](docs/AGENT_SUPPORT.md#codex-on-windows) for prerequisites, environment setup, and connection checks.
