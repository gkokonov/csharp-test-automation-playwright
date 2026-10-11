# BDD application review checklist

Read CsharpTestAutomation.Bdd.Tests/AGENTS.md, C#, shared test and BDD rules,
applicable API/UI rules, layer architecture and the affected feature specification.
Check the independent application layer and framework-only project reference,
business-readable scenario parity, Steps conventions, typed state, hook ownership,
lazy service needs, LIFO cleanup, client lifetime, and Allure.Reqnroll integration.
Do not impose NUnit fixture inheritance or add an extra workflow layer.
Check observable UI readiness, API contracts, isolated data, parallel safety,
diagnostics ordering and honest live-service evidence.
Apply .agents/review/REVIEW.md for evidence, scope, and reporting.
