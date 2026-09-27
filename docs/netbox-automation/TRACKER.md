# NetBox Automation — Implementation Tracker

Status legend: `TODO` · `IN PROGRESS` · `BLOCKED` · `DONE`

Update this file as implementation proceeds. Do not start implementation
until this tracker and the specs in this folder are reviewed.

## Phase 0 — Cleanup

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 0.1 | Extract good CPF-based patterns from instruction files into generic/NetBox examples | TODO | `docs/API_TESTING_ARCHITECTURE.md`, `docs/UI_TESTING_ARCHITECTURE.md`, `.agents/rules/test-automation.md` — see `01-infrastructure-spec.md` §0.a |
| 0.2 | Remove obsolete CPF API clients/DTOs/builders/tests | TODO | See `01-infrastructure-spec.md` §0.b |
| 0.3 | Remove obsolete CPF DB queries/row DTOs | TODO | `Database/CPF/**` |
| 0.4 | Remove obsolete CPF UI page objects/components | TODO | Verify `ExampleUiTests` template is unaffected |
| 0.5 | Remove CPF entries from `appsettings.json` / `appsettings.local.json` | TODO | Keep structure for reuse by NetBox config |
| 0.6 | Re-evaluate `docs/API_TESTING_ARCHITECTURE.md`, `docs/UI_TESTING_ARCHITECTURE.md`, `.agents/rules/test-automation.md` for dangling references and fix | TODO | See `01-infrastructure-spec.md` §0.c; gate before Phase 1 |
| 0.7 | Confirm no residual `Cpf`/`cpg` references repo-wide (`grep_search`) | TODO | Gate before Phase 1 |

## Phase 1 — Infrastructure

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 1.1 | `NetBoxConfigurationDTO` + `ExtendedConfiguration` wiring | TODO | Fold UI credentials into `NetBox`; simplify `UiConfigurationDTO` away from its CPF-only shape (resolved in `01-infrastructure-spec.md`) |
| 1.2 | `appsettings.json` / `appsettings.local.json` NetBox + DbSettings entries | TODO | Both files must carry the **same key set**; only secret values differ (`PLACEHOLDER_PASSWORD` vs real) — see `01-infrastructure-spec.md` Configuration section |
| 1.3 | Health check (NetBox root, `/api/`, Postgres) fails fast | TODO | Model on `GlobalSetupFixture` |
| 1.4 | `NetBoxAuthClient` (token provisioning) — design with software-design-principles skill | TODO | `Authorization: Token <value>` scheme |
| 1.5 | `NetBoxSession` (cached token holder) + `NetBoxTokenAuthenticator` (`IAuthenticator`) | TODO | Keep provisioning/caching/header-application as separate collaborators (SRP) |
| 1.6 | `SitesApiClient` | TODO | |
| 1.7 | `DevicesApiClient`, `DeviceTypesApiClient`, `ManufacturersApiClient`, `DeviceRolesApiClient` | TODO | |
| 1.8 | `IpamApiClient` (prefixes + IP addresses) | TODO | |
| 1.9 | DTOs for Sites/Devices/DeviceTypes/Manufacturers/DeviceRoles/Ipam | TODO | **Blocked until** field casing is confirmed against `/api/schema/` — see `01-infrastructure-spec.md` Open Item 2 |
| 1.10 | DTO builders (`CreateSiteDtoBuilder`, etc.) | TODO | Unique `auto-{feature}-{guid}` defaults |
| 1.11 | `SitesDatabaseRepository`, `DevicesDatabaseRepository`, IPAM DB repository | TODO | Read-only, parameterized |
| 1.12 | `NetBoxLoginPage`, `SitesListPage`, `SiteEditPage` (create + update), `SiteDetailsPage` (incl. delete) | TODO | Semantic locators only |
| 1.13 | Decide + implement shared UI storage-state vs per-test login | TODO | Default: shared storage state via `OneTimeSetUp` |

## Phase 2 — Site API Tests

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 2.1 | `CreateSite_ShouldReturnCreatedSite` | TODO | |
| 2.2 | `CreateSite_ShouldPersistSiteInDatabase` | TODO | |
| 2.3 | `GetSite_ShouldReturnMatchingSiteBySlug` | TODO | |
| 2.4 | `UpdateSite_ShouldChangeStatus` | TODO | |
| 2.5 | `DeleteSite_ShouldRemoveSite` | TODO | |
| 2.6 | All Site API tests green x3 consecutive runs | TODO | Gate before Phase 3 |

## Phase 3 — Site UI Tests

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 3.1 | `CreateSite_ShouldPersistAcrossLayers` (UI → API → DB validation) | TODO | |
| 3.2 | `UpdateSite_ShouldReflectChangedStatusAcrossLayers` | TODO | Site created via API setup |
| 3.3 | `DeleteSite_ShouldRemoveSiteAcrossLayers` | TODO | Site created via API setup; no cleanup needed (test deletes it) |
| 3.4 | API cleanup wired and verified idempotent (create/update tests) | TODO | |
| 3.5 | All 3 Site UI tests green x3 consecutive headless runs | TODO | Gate before Phase 4 |

## Phase 4 — Device Automation

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 4.1 | Prerequisite builders (Manufacturer, DeviceType, DeviceRole) | TODO | |
| 4.2 | Device API CRUD tests | TODO | |
| 4.3 | Device filter/search test | TODO | |
| 4.4 | `DevicesListPage`, `DeviceEditPage`, `DeviceDetailsPage` | TODO | Semantic locators only |
| 4.5 | `CreateDevice_ShouldPersistAcrossLayers` UI test | TODO | Prerequisites via API only |
| 4.6 | Cleanup order verified even on failure | TODO | |
| 4.7 | Green x3 consecutive runs (API + UI) | TODO | Gate before Phase 5 |

## Phase 5 — IPAM Automation

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 5.1 | Prefix API CRUD tests (dynamic RFC1918 ranges) | TODO | |
| 5.2 | IP Address API CRUD tests | TODO | |
| 5.3 | NetBox-specific IPAM behavior assertion (status transition) | TODO | |
| 5.4 | Parallel-safety check (no reused CIDRs) | TODO | |
| 5.5 | `PrefixesPage`, `PrefixEditPage`, `PrefixDetailsPage` | TODO | Semantic locators only |
| 5.6 | `CreatePrefix_ShouldPersistAcrossLayers` UI test | TODO | 5th and final planned UI test |
| 5.7 | Green x3 consecutive runs (API + UI) | TODO | |

## Cross-Cutting Gates (recheck at end of each phase)

- [ ] `dotnet build .\CsharpTestAutomation.slnx` is warning-clean.
- [ ] No secrets (password, API token) appear in logs or Allure attachments.
- [ ] Every created record registers cleanup at creation time.
- [ ] No fixed/shared test data names across parallel-safe tests.
- [ ] Allure metadata complete (suite/feature/story/severity/owner) on every new test.
- [ ] No duplicate infrastructure introduced beyond what's spec'd here.
- [ ] Owning instruction file (`.agents/rules/test-automation.md`, `docs/API_TESTING_ARCHITECTURE.md`, `docs/UI_TESTING_ARCHITECTURE.md`) updated if a new pattern is introduced.
- [ ] Exactly 5 UI tests implemented by end of Phase 5 (3 Site, 1 Device, 1 Prefix) per the confirmed UI-coverage decision below.

## Decision Log

| Date | Decision | Made By |
| --- | --- | --- |
| 2026-09-27 | Reuse existing 3 projects instead of new `NetBox.*.Tests` projects | Confirmed by user |
| 2026-09-27 | Remove CPF assets, but extract good patterns into instructions first, then re-evaluate those instructions for dangling references | Confirmed by user |
| 2026-09-27 | Specs stored under `docs/netbox-automation/` | Confirmed by user |
| 2026-09-27 | New NetBox-specific auth flow, not reusing MSAL `BootstrapSession`; designed using the software-design-principles skill (separate provisioning/session/authenticator collaborators) | Confirmed by user |
| 2026-09-27 | Secrets via `CsharpTestAutomation.Tests/appsettings.local.json`, reusing the file already used for CPF | Confirmed by user |
| 2026-09-27 | Plan 5 UI tests total (Site create/update/delete, Device create, Prefix create) instead of the source attachment's single-flow recommendation | Confirmed by user (requested "at least 3-5") |
