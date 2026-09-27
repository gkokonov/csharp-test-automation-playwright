# NetBox Automation — Implementation Tracker

Status legend: `TODO` · `IN PROGRESS` · `BLOCKED` · `DONE`

Update this file as implementation proceeds. Do not start implementation
until this tracker and the specs in this folder are reviewed.

## Phase 0 — Cleanup

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 0.1 | Replace application-specific instruction examples with generic patterns | DONE | Architecture and authoring instructions now use illustrative resource names. |
| 0.2 | Remove obsolete API clients/DTOs/builders/tests | DONE | Removed the prior application's typed client, DTOs, builder, and API fixtures. |
| 0.3 | Remove obsolete DB queries/row DTOs | DONE | Removed application-specific database query and row DTOs. |
| 0.4 | Remove obsolete UI tests and page objects/components | DONE | Removed the app-bound bootstrap UI test; no app-specific page object/component remained. `ExampleUiTests` is retained. |
| 0.5 | Remove application entries from `appsettings.json` / `appsettings.local.json` | DONE | Retained reusable browser, API logging, and empty DB configuration structure. |
| 0.6 | Re-evaluate architecture and authoring instructions for dangling references | DONE | Re-read all three files; obsolete file and class references were replaced. |
| 0.7 | Confirm no retired application identifiers remain repo-wide | DONE | Repository-wide source search returned no matches. |

## Phase 1 — Infrastructure

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 1.1 | `NetBoxConfigurationDTO` + `ExtendedConfiguration` wiring | DONE | `NetBox` settings bound and confirmed live (username/password load from `appsettings.local.json` as expected); `UiConfigurationDTO` simplification deferred to the UI infra follow-up (see below) |
| 1.2 | `appsettings.json` / `appsettings.local.json` NetBox + DbSettings entries | DONE | Both files carry the same key set; local credentials are in the ignored file |
| 1.3 | Health check (NetBox root, `/api/`, Postgres) fails fast | DONE | Validated live. Fixed a false failure: NetBox's `/api/` root returns `403` when unauthenticated (login-required mode) — `HttpClientExtensions.EnsureAvailableAsync` now only fails on 5xx, since a 4xx still proves the dependency is reachable |
| 1.4 | `NetBoxAuthClient` (token provisioning) — design with software-design-principles skill | DONE | Validated live against NetBox 4.7. Fixed two live-only defects: (1) NetBox's dev server rejects a chunked-transfer POST body — switched from `PostAsJsonAsync` to a buffered `StringContent`; (2) NetBox 4.x defaults token provisioning to "v2" split key/secret tokens, which need a `Bearer <key>.<token>` header — the client now requests `version: 1` explicitly and reads the `token` field (not `key`) to match `NetBoxTokenAuthenticator`'s `Authorization: Token <value>` scheme |
| 1.5 | `NetBoxSession` (cached token holder) + `NetBoxTokenAuthenticator` (`IAuthenticator`) | DONE | Lazy token cache and `Authorization: Token` authenticator validated live end-to-end |
| 1.6 | `SitesApiClient` | DONE | Create/Get/FindBySlug/Update(PATCH)/Delete validated live |
| 1.7 | `DevicesApiClient`, `DeviceTypesApiClient`, `ManufacturersApiClient`, `DeviceRolesApiClient` | DONE | Validated live, including the Device→DeviceType→Manufacturer/DeviceRole/Site prerequisite chain |
| 1.8 | `IpamApiClient` (prefixes + IP addresses) | DONE | Prefix and IP Address create/get/update/delete validated live |
| 1.9 | DTOs for Sites/Devices/DeviceTypes/Manufacturers/DeviceRoles/Ipam | DONE | Field casing confirmed against `NetBox REST API (4.7).json` and the live instance. Response `status` is a nested `{value,label}` object (`StatusFieldDto`); write payloads use a plain string. List and detail share one schema per resource (no separate `ListItemDto`) |
| 1.10 | DTO builders (`CreateSiteDtoBuilder`, etc.) | DONE | `auto-{feature}-{guid}` defaults via shared `TestData/NetBox/NetBoxTestData`; FK-only builders (Device, DeviceType) intentionally leave prerequisite ids unset |
| 1.11 | `SitesDatabaseRepository`, `DevicesDatabaseRepository`, IPAM DB repository | DONE | Read-only, parameterized; validated live against `dcim_site`, `dcim_device`, `ipam_prefix`, `ipam_ipaddress` |
| 1.12 | `NetBoxLoginPage`, `SitesListPage`, `SiteEditPage` (create + update), `SiteDetailsPage` (incl. delete) | TODO | Deferred — moved to a new UI infrastructure spec (see below), tracked separately from the API/DB infra in this phase |
| 1.13 | Decide + implement shared UI storage-state vs per-test login | TODO | Deferred — moved to the new UI infrastructure spec alongside 1.12 |

**Scope note (2026-09-27):** Phase 1 API/DB infrastructure (1.1–1.11) is complete and was validated end-to-end against the live local NetBox instance (create → DB read → update → find → delete across Site, Manufacturer, DeviceRole, DeviceType, Device, Prefix, and IP Address, with full cleanup). UI page objects and the storage-state decision (1.12–1.13) are deferred to a follow-up UI infrastructure spec, to be authored before Phase 3 (Site UI tests) starts.

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
| 2026-09-27 | Remove legacy application assets after extracting reusable patterns and checking instructions for dangling references | Confirmed by user |
| 2026-09-27 | Specs stored under `docs/netbox-automation/` | Confirmed by user |
| 2026-09-27 | New NetBox-specific auth flow, not reusing MSAL `BootstrapSession`; designed using the software-design-principles skill (separate provisioning/session/authenticator collaborators) | Confirmed by user |
| 2026-09-27 | Secrets via the existing ignored `CsharpTestAutomation.Tests/appsettings.local.json` file | Confirmed by user |
| 2026-09-27 | Plan 5 UI tests total (Site create/update/delete, Device create, Prefix create) instead of the source attachment's single-flow recommendation | Confirmed by user (requested "at least 3-5") |
| 2026-09-27 | Phase 1 split: API/DB infra (1.1-1.11) implemented and live-validated in this session; UI page objects + storage-state decision (1.12-1.13) deferred to a follow-up UI infrastructure spec | Confirmed by user |
