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
| 1.1 | `NetBoxConfigurationDTO` + `ExtendedConfiguration` wiring | DONE | NetBox settings are bound; `UiConfigurationDTO` now contains only the shared storage-state directory |
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
| 1.12 | `NetBoxLoginPage`, `SitesListPage`, `SiteEditPage` (create + update), `SiteDetailsPage` (incl. delete) | DONE | Added semantic page actions and a shared delete-confirmation component |
| 1.13 | Decide + implement shared UI storage-state vs per-test login | DONE | One `OneTimeSetUp` login captures storage state for each UI test context; `OneTimeTearDown` removes the state file. This follows the spec's default recommendation |

**Scope note (2026-10-01):** Phase 1 infrastructure (1.1–1.13) is implemented. NetBox UI tests have not been added yet; no NetBox application tests were available to run for this phase.

## Phase 2 — Site API Tests

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 2.1 | `CreateSite_ShouldReturnCreatedSite` | DONE | `Tests/API/SitesApiTests.cs`; derives from `ApiTestBase` (corrected from an initial plain-`[TestFixture]` implementation per user feedback — every API fixture now derives from `ApiTestBase`) |
| 2.2 | `CreateSite_ShouldPersistSiteInDatabase` | DONE | Verifies both layers: REST GET equivalence to the create response, and a direct `SitesDatabaseRepository` Postgres row check (confirmed by user) |
| 2.3 | `GetSite_ShouldReturnMatchingSiteBySlug` | DONE | |
| 2.4 | `UpdateSite_ShouldChangeStatus` | DONE | Added `UpdateSiteDtoBuilder` (`planned` status + updated description) for consistency with the builder convention |
| 2.5 | `DeleteSite_ShouldRemoveSite` | DONE | |
| 2.6 | All Site API tests green x3 consecutive runs | DONE | 5/5 passed on 3 consecutive live runs against the local NetBox instance (re-verified after the `ApiTestBase`/Bogus rework) |

## Phase 3 — Site UI Tests

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 3.1 | `CreateSite_ShouldPersistAcrossLayers` (UI → API → DB validation) | DONE | UI creates via Playwright (slug filled before name to prevent NetBox JS auto-generation); API + DB layers asserted post-create |
| 3.2 | `UpdateSite_ShouldReflectChangedStatusAcrossLayers` | DONE | Site created via API setup; UI updates status + description; API + DB layers verified |
| 3.3 | `DeleteSite_ShouldRemoveSiteAcrossLayers` | DONE | Site created via API setup; UI deletes; list page, API 404, DB null all verified |
| 3.4 | API cleanup wired and verified idempotent (create/update tests) | DONE | `ScenarioCleanupActions` registered at creation time; `DeleteSiteAsync` tolerates 404 |
| 3.5 | All 3 Site UI tests green x3 consecutive headless runs | DONE | 3/3 passed on 3 consecutive runs against local NetBox instance |

## Phase 4 — Device Automation

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 4.1 | Prerequisite builders (Manufacturer, DeviceType, DeviceRole) | DONE | Reused the builders completed in Phase 1; `Steps/API/NetBox/DeviceSteps` creates owned prerequisites and registers cleanup immediately. |
| 4.2 | Device API CRUD tests | DONE | `DevicesApiTests`: create, exact-name lookup, PATCH with/without site change, and delete; persisted Device fields checked against PostgreSQL. Added optional `Site` to PATCH DTO and an update builder. |
| 4.3 | Device filter/search test | DONE | `site_id` filter tested with two matching Devices and one Device at another owned site; exact-name search is covered separately. |
| 4.4 | `DevicesListPage`, `DeviceEditPage`, `DeviceDetailsPage` | DONE | Semantic headings, comboboxes, options, row headers, and links; locators checked against live NetBox 4.7. |
| 4.5 | `CreateDevice_ShouldPersistAcrossLayers` UI test | DONE | One UI create scenario passed live with API-only prerequisites, semantic detail assertions, and REST/PostgreSQL checks. |
| 4.6 | Cleanup order verified even on failure | DONE | Two API interruption cases passed. The initial UI assertion failure also deleted Device, Site, Role, DeviceType, then Manufacturer successfully. |
| 4.7 | Green x3 consecutive runs (API + UI) | DONE | 17/17 passed on three consecutive live headless runs: 8 Device API cases, 1 Device UI case, and 8 existing Site regressions. Results: `artifacts/netbox-phase4/netbox-final-{1,2,3}.trx`. |

## Phase 5 — IPAM Automation

| # | Task | Status | Notes |
| --- | --- | --- | --- |
| 5.1 | Prefix API CRUD tests (dynamic RFC1918 ranges) | DONE | Four `PrefixesApiTests` scenarios: create, exact CIDR filter, active-to-reserved PATCH, and delete. REST re-fetch and complete represented DB fields checked on writes. |
| 5.2 | IP Address API CRUD tests | DONE | Four `IpAddressesApiTests` scenarios, each with an owned prerequisite Prefix and an address inside its /24. DB projection retains the address mask. |
| 5.3 | NetBox-specific IPAM behavior assertion (status transition) | DONE | Prefix and IP Address updates verify `active` to `reserved`, the `Reserved` label, unchanged CIDR, and REST/DB persistence. |
| 5.4 | Parallel-safety check (no reused CIDRs) | DONE | Generated /24 CIDRs are reserved under a lock for the process lifetime. Full runs use `NUnit.NumberOfTestWorkers=3`; TRX timestamps show overlapping Prefix/IP Address fixtures. Separate processes do not share this reservation. |
| 5.5 | `PrefixesPage`, `PrefixEditPage`, `PrefixDetailsPage` | DONE | Semantic locators validated with live Playwright CLI. Status option names include help text; selection matches the status at the start of the option name. |
| 5.6 | `CreatePrefix_ShouldPersistAcrossLayers` UI test | DONE | Fifth planned UI scenario: create, semantic detail checks, REST filter lookup, and PostgreSQL verification. Owned lookup cleanup registered before submission, then id cleanup after lookup. |
| 5.7 | Green x3 consecutive runs (API + UI) | DONE | All 26 NetBox cases passed on three consecutive live headless runs with three NUnit workers: 9 IPAM cases and 17 existing Site/Device regressions. Results: `artifacts/netbox-phase5/netbox-final-{1,2,3}.trx`. |

**Phase 5 review (2026-10-04):** One read-only reviewer subagent pass completed.
The main agent fixed its one finding: UI Prefix lookup and fallback cleanup now
filter by both CIDR and unique description on the server, so unrelated duplicate
CIDRs cannot push the owned record onto a later result page. All 9 IPAM cases
then passed on three consecutive live headless runs with three NUnit workers.
Results: `artifacts/netbox-phase5/ipam-review-fix-{1,2,3}.trx`.
Debug solution and Release application builds, scoped formatting/analyzer
verification, instruction size/link checks, and `git diff --check` passed.
The previous code index's AST-only update completed; documentation extraction
was not run. Codebase Memory MCP replaced that index on 2026-10-06.

**Site and teardown review fixes (2026-10-05):** Fixed review findings 2, 3,
and 5. `SiteSteps` registers API cleanup before response assertions and UI
lookup cleanup before submission. `TestBase` disposes and clears registered
services after cleanup errors while preserving the cleanup failure. Site DB
checks now include all represented persisted fields, including description;
API update checks the follow-up GET status, and API delete checks row removal.
The Site specification records the corrected behavior.

Validation passed:

- Application Debug and Release builds: zero warnings and errors.
- Scoped formatter/analyzer verification, instruction size/link checks, the
  changed Site specification's local links, and `git diff --check`.
- Three consecutive live headless passes for all eight Site API/UI tests,
  with three NUnit workers. The middle run also passed all 26 NetBox cases,
  including Device cleanup interruption scenarios and IPAM regressions.
  Results: `artifacts/netbox-review-fixes/site-run-1.trx`,
  `artifacts/netbox-review-fixes/netbox-run-2.trx`, and
  `artifacts/netbox-review-fixes/site-run-3.trx`.
- Local failure-path probes: four concurrent teardown probes with two cycles
  each verify cleanup ordering, client disposal, container clearing, and the
  original error. In-process HTTP probes verify API cleanup registration on
  an unexpected response status and owned-slug UI cleanup with idempotent 404
  handling. Source and output: `artifacts/netbox-review-fixes/local-probe/`
  and `artifacts/netbox-review-fixes/local-checks.txt`. These are ignored local
  validation artifacts, not permanent framework self-tests.
- Historical AST-only code index update completed: 1,773 nodes, 3,402 edges, and 119
  communities. Documentation extraction was not run; saved community labels
  need a separate refresh.

Resolved validation issue: a Debug rebuild attempted while the live test
host held the assembly failed with MSB3027/MSB3021. Rebuilding after that test
process exited passed without warnings or errors. No failed checks remain.

## Cross-Cutting Gates (recheck at end of each phase)

- [x] `dotnet build .\CsharpTestAutomation.slnx` is warning-clean.
- [x] Phase 4 API request/response logs and Allure attachments redact credentials. Startup configuration logging writes raw values as expected by the user; configuration redaction is deferred. Historical ignored Allure output predating this phase still contains unredacted authorization headers.
- [x] Every created record registers cleanup at creation time.
- [x] No fixed/shared test data names across parallel-safe tests.
- [x] Allure metadata complete (suite/feature/story/severity/owner) on every new test.
- [x] No duplicate infrastructure introduced beyond what's spec'd here.
- [x] Owning instruction file (`.agents/rules/test-automation.md`, `docs/API_TESTING_ARCHITECTURE.md`, `docs/UI_TESTING_ARCHITECTURE.md`) updated if a new pattern is introduced.
- [x] Exactly 5 UI tests implemented by end of Phase 5 (3 Site, 1 Device, 1 Prefix) per the confirmed UI-coverage decision below.

**Phase 4 review (2026-10-03):** One read-only reviewer subagent pass completed.
The main agent fixed its finding: cleanup registration is in creation order,
while LIFO execution deletes dependents first. API redaction is enabled in the
committed and ignored local configuration. Phase 4 attachments generated before
that correction were sanitized. Formatting/analyzer verification and the
solution build passed. Four planned UI scenarios now exist; Prefix remains
Phase 5 work.

**Steps refactor (2026-10-03):** Device prerequisite operations and their result
record now live in `Steps/API/NetBox/` as `DeviceSteps` and `DevicePrerequisites`.
Both Device fixtures use the new namespace. The root instructions, authoring
rules, architecture references, and Device spec record this convention.
The solution build and formatter verification passed; all 9 existing Device
API/UI cases passed in the focused live headless run. Results:
`artifacts/netbox-phase4/device-steps-refactor.trx`.

## Decision Log

| Date | Decision | Made By |
| --- | --- | --- |
| 2026-09-27 | Reuse existing 3 projects instead of new `NetBox.*.Tests` projects | Confirmed by user |
| 2026-09-27 | Remove legacy application assets after extracting reusable patterns and checking instructions for dangling references | Confirmed by user |
| 2026-09-27 | Specs stored under `docs/netbox-automation/` | Confirmed by user |
| 2026-09-27 | New NetBox-specific API authentication flow, separate from federated UI authentication; designed using the software-design-principles skill (separate provisioning/session/authenticator collaborators) | Confirmed by user |
| 2026-09-27 | Secrets via the existing ignored `CsharpTestAutomation.Tests/appsettings.local.json` file | Confirmed by user |
| 2026-09-27 | Plan 5 UI tests total (Site create/update/delete, Device create, Prefix create) instead of the source attachment's single-flow recommendation | Confirmed by user (requested "at least 3-5") |
| 2026-09-27 | Phase 1 API/DB infrastructure (1.1-1.11) implemented; UI objects and storage-state decision were initially deferred | Confirmed by user |
| 2026-10-01 | Use one shared Playwright storage state per run, captured in `OneTimeSetUp` and removed after UI tests | Spec default |
| 2026-10-01 | `CreateSite_ShouldPersistSiteInDatabase` verifies persistence at both layers: a REST GET re-fetch equivalence check, and a direct Postgres row check via `SitesDatabaseRepository` | Confirmed by user |
| 2026-10-01 | Every API test fixture derives from `ApiTestBase` (superseding `02-site-management-spec.md`'s plain-`[TestFixture]` suggestion) — it registers typed clients via `ApiTestBase.RegisterClient`/`GetClient<T>` (added to remove direct `TestContainer` use from fixtures) so disposal happens after `ScenarioCleanupActions` runs, and uses the inherited `ScenarioCleanupActions`/`NetBoxAuthenticator` rather than fixture-local copies | Confirmed by user |
| 2026-10-01 | Test-data value generation (unique names/slugs/prefixes/IPs) prefers Bogus (`Faker`/`Randomizer`) over raw `Guid`/`Random`; fixed domain constants (status enum values, human-readable description labels) stay as plain constants | Confirmed by user |
| 2026-10-03 | Startup configuration logging writes values without redaction as expected. Correct its comment; configuration redaction is deferred until requested. | Confirmed by user |
| 2026-10-03 | Reusable workflows and prerequisite operations live in `Steps/API/<App>/` or `Steps/UI/<App>/`, chosen by their implementation. Classes use the `Steps` suffix. Device prerequisite operations and their result record live together in `Steps/API/NetBox/`; constants and generated values remain in `TestData/`. | Confirmed by user |
