# Application Test Scope

Before changing this project, read the repository [AGENTS.md](../AGENTS.md) and the canonical [test-automation rules](../.agents/rules/test-automation.md). The rules file contains the detailed conventions for API clients, DTOs, test data, NUnit, Allure, page objects, and UI tests.

Use `CsharpTestAutomation.Tests.csproj` for application test builds. Run live API or UI tests only when their configured external services and credentials are available.

Use the root test naming convention and the canonical test-authoring rules in
`.agents/rules/test-automation.md` for test names and assertions.
