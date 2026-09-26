# CsharpTestAutomation.Framework

Reusable .NET 10 infrastructure for test automation. The library provides configuration, API clients and logging, database helpers, Playwright browser lifecycle management, and Allure reporting utilities.

The package is designed to be consumed by test projects. Application endpoints, credentials, environment-specific settings, DTOs, and test fixtures belong in the consuming project, not in this package.

## Build and Package

```pwsh
dotnet build .\CsharpTestAutomation.Framework\CsharpTestAutomation.Framework.csproj
dotnet pack .\CsharpTestAutomation.Framework\CsharpTestAutomation.Framework.csproj
```

The project uses `CoreConfiguration` as its configuration contract. Consumers provide their own `appsettings.json` or other configuration source. See the repository [API testing architecture](../docs/API_TESTING_ARCHITECTURE.md) and [UI testing architecture](../docs/UI_TESTING_ARCHITECTURE.md) for usage patterns.
