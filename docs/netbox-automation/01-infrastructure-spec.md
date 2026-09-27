# 01 — Infrastructure Spec

## Phase 0 — Preserve Patterns, Then Remove Obsolete Application Assets

The prior application no longer exists. Its code is obsolete, but three
harness instruction files currently use application-specific classes as their
**worked examples** and must not be left pointing at deleted files:

- `docs/API_TESTING_ARCHITECTURE.md` — used application-specific client, test, DTO, builder, and database query names.
- `docs/UI_TESTING_ARCHITECTURE.md` — used an application-specific Allure owner in its example fixture.
- `.agents/rules/test-automation.md` — used application-specific query, row DTO, test-data, and builder names.

### 0.a — Extract good patterns before deleting code

For each instruction file above, replace the application-specific worked example with
a generic/anonymized illustration that does not name a real file (do **not**
wait for a NetBox equivalent to exist — that would make Phase 0 depend on
Phase 1/2 completing first, which defeats the point of a cleanup phase that
gates the rest of the work). Once the first NetBox client/test lands later,
revisit the instruction file and swap the generic illustration for a concrete
NetBox example if that improves clarity — track that as a follow-up note in
`TRACKER.md`, not as a Phase 0 blocker. Patterns worth explicitly preserving
because they are good, reusable conventions independent of the prior application:

- Factory-owned, disposable typed API client shape (`IRestClientFactory` injected, owns `IRestClient`, `IDisposable`).
- DTO folder-per-resource layout and the `{Resource}ListItemDto`/`{Resource}DetailDto`/`Create{Resource}Dto` naming table.
- `BaseBuilder<T>`-backed DTO builder pattern.
- `RequireDbData<T>` usage for seeded/read-only environment data.
- `Database/<App>/Queries` + `Database/<App>/DTO` folder split with SQL-alias-to-C#-member convention.

### 0.b — Remove obsolete application code

- Application-specific API fixtures, typed clients, DTOs, and DTO builders
- Application-specific database queries and row DTOs
- Application-specific test data not reused by framework self-tests
- Application-specific page objects/components and tests that depend on the retired application
- Application URL, authentication users, database credentials, and API service entries in `appsettings.json` and `appsettings.local.json`

**Keep**: `ApiTestBase`, `TestBase`, `UiTestBase`, `TestContainer`, `GlobalSetupFixture`, `PostgreSqlConnectionPool`, `ExtendedConfiguration` shape (fields get repurposed for NetBox, not deleted), the `ExampleUiTests` template, and anything in `CsharpTestAutomation.Framework` (the framework has no application-specific code per its own layering rule).

### 0.c — Re-evaluate instructions after deletion

After deletion, re-read `docs/API_TESTING_ARCHITECTURE.md`, `docs/UI_TESTING_ARCHITECTURE.md`, and `.agents/rules/test-automation.md` end-to-end and confirm every file/class/path they cite still exists. Search the whole repository for retired application identifiers (not just in `CsharpTestAutomation.Tests`) before considering this phase done — do not rely on the lists above being exhaustive.

## Configuration

Extend `CsharpTestAutomation.Tests/Configurations/Models/` with a `NetBoxConfigurationDTO`:

```text
NetBox
  BaseUrl            (UI base, e.g. http://localhost:8000/)
  ApiBaseUrl          (e.g. http://localhost:8000/api/)
  Username
  Password
  ApiToken            (nullable — provisioned at runtime if absent)
```

Reuse the existing `DBConfigurationDTO` shape for Postgres (`DbSettings.Database[]` with `DbName=netbox`, `Username=netbox`, `Password`). No `PostgreSql.EnableSslMode` needed for local Docker Postgres (set `false`).

Add `NetBoxSettings` to `ExtendedConfiguration`. **Resolved**: fold NetBox UI credentials (`Username`/`Password`) into the same `NetBox` config block used for API access — do not keep a separate provider-specific `Ui.Authentication.Users[]` list. Repurpose `UiConfigurationDTO` down to only what NetBox's single-admin-user flow needs (e.g. `StorageStateDirectory`); `DbSettings` is already generic and is reused as-is. This avoids keeping two parallel application-specific configuration shapes.

`appsettings.json` (checked in, secret fields use a placeholder value):

```json
{
  "NetBox": {
    "BaseUrl": "http://localhost:8000/",
    "ApiBaseUrl": "http://localhost:8000/api/",
    "Username": "admin",
    "Password": "PLACEHOLDER_PASSWORD"
  },
  "DbSettings": {
    "Server": "localhost",
    "Port": 5432,
    "Database": [ { "DbName": "netbox", "Username": "netbox", "Password": "PLACEHOLDER_PASSWORD" } ],
    "PostgreSql": { "EnableSslMode": false }
  },
  "Api": {
    "Services": {
      "netbox": { "BaseUrl": "http://localhost:8000/api/" }
    }
  }
}
```

`appsettings.local.json` (gitignored, developer-supplied) **mirrors every key in `appsettings.json`** — same full structure, not just the secret subset — with real local values substituted for the placeholders:

```json
{
  "NetBox": {
    "BaseUrl": "http://localhost:8000/",
    "ApiBaseUrl": "http://localhost:8000/api/",
    "Username": "admin",
    "Password": "admin"
  },
  "DbSettings": {
    "Server": "localhost",
    "Port": 5432,
    "Database": [ { "DbName": "netbox", "Username": "netbox", "Password": "J5brHrAXFLQSif0K" } ],
    "PostgreSql": { "EnableSslMode": false }
  },
  "Api": {
    "Services": {
      "netbox": { "BaseUrl": "http://localhost:8000/api/" }
    }
  }
}
```

`appsettings.json` provides committed defaults. In local mode, `appsettings.local.json` is loaded after it and can override those values. Put local credentials and machine-specific settings in `appsettings.local.json`. Keep `NetBox.ApiBaseUrl` and `Api.Services.netbox.BaseUrl` aligned because setup checks and typed API clients read them separately.

For local development, leave `Environment` unset and use `appsettings.local.json`; no NetBox-specific environment-variable overrides are supported. For environment-specific runs, `AppConfiguration` selects `appsettings.{Environment}.json` and applies the framework's standard environment-variable provider.

**Fail fast**: add a startup/`OneTimeSetUp` health check (NetBox root, `/api/`, and Postgres reachability) consistent with "the test framework should fail fast if required dependencies are unavailable." Model this after `GlobalSetupFixture`.

## API Token Provisioning and Auth Design

Design this with the **software-design-principles** skill (SRP, DIP,
composition over a single god class) rather than folding provisioning, header
application, and caching into one type. Three separate, independently
testable collaborators:

```text
NetBoxAuthClient        → POST users/tokens/provision/ (username/password in, token out). Pure HTTP concern, no caching.
NetBoxSession           → holds the token for the run (lazy-provisions via NetBoxAuthClient on first access, or
                           short-circuits to NetBox.ApiToken from configuration when already supplied). Single
                           responsibility: "what is today's valid token", analogous in role (not shape) to BootstrapSession.
NetBoxTokenAuthenticator → RestSharp IAuthenticator that reads NetBoxSession's current token and applies it as
                           `Authorization: Token <value>` (NetBox's own scheme, not Bearer/JwtAuthenticator).
```

Rationale for the split: `NetBoxAuthClient` changes only if the provisioning
endpoint/contract changes; `NetBoxSession` changes only if caching/refresh
policy changes; `NetBoxTokenAuthenticator` changes only if RestSharp's
`IAuthenticator` contract or NetBox's header scheme changes. Each depends on
an abstraction it needs (`NetBoxTokenAuthenticator` depends on `NetBoxSession`,
not on `NetBoxAuthClient` or HTTP details) rather than on concrete
provisioning mechanics \u2014 keeps the authenticator trivially fakeable in
framework/client unit tests without a real HTTP call.

Never log the token or password; ensure `ApiLogSanitizer` redacts the
`Authorization` header (verify default redaction covers the `Token` scheme,
not just `Bearer`).

## REST API Layer

Add service entry `Api.Services.netbox` (`BaseUrl` from config). One typed client per domain, following `docs/API_TESTING_ARCHITECTURE.md` §3 (factory-owned, disposable, `IRestClientFactory` injected):

- `SitesApiClient` — `CreateSiteAsync`, `GetSiteAsync(id)`, `FindSitesBySlugAsync(slug)`, `UpdateSiteAsync` (PATCH), `DeleteSiteAsync`
- `DevicesApiClient` — `CreateDeviceAsync`, `GetDeviceAsync(id)`, `FindDevicesAsync(filter)`, `UpdateDeviceAsync`, `DeleteDeviceAsync`
- `DeviceTypesApiClient` / `ManufacturersApiClient` / `DeviceRolesApiClient` — minimal create/get, for prerequisite setup only (no full CRUD test coverage required)
- `IpamApiClient` — prefixes: `CreatePrefixAsync`, `GetPrefixAsync`, `UpdatePrefixAsync`, `DeletePrefixAsync`; IP addresses: `CreateIpAddressAsync`, `GetIpAddressAsync`, `UpdateIpAddressAsync`, `DeleteIpAddressAsync`

Each client is `sealed`, takes `IRestClientFactory` + optional `IAuthenticator`, and disposes its own `IRestClient`. Endpoints per attachment:

```text
/api/dcim/sites/
/api/dcim/devices/
/api/dcim/device-types/
/api/dcim/manufacturers/
/api/dcim/device-roles/
/api/ipam/prefixes/
/api/ipam/ip-addresses/
```

### DTOs (`API/DTOs/<Resource>/`)

**Blocking dependency**: confirm actual field casing against the running SUT's `/api/schema/` before writing any DTO (see Open Item 2 below) — do not start task 1.9 in the tracker until this is confirmed, since guessing wrong means reworking every `[JsonPropertyName]` afterward.

Follow the naming table in `docs/API_TESTING_ARCHITECTURE.md` §5:

```text
API/DTOs/
  Sites/       SiteListItemDto, SiteDetailDto, CreateSiteDto, UpdateSiteDto
  Devices/     DeviceListItemDto, DeviceDetailDto, CreateDeviceDto, UpdateDeviceDto
  DeviceTypes/ DeviceTypeDto, CreateDeviceTypeDto
  Manufacturers/ ManufacturerDto, CreateManufacturerDto
  DeviceRoles/ DeviceRoleDto, CreateDeviceRoleDto
  Ipam/        PrefixDto, CreatePrefixDto, UpdatePrefixDto, IpAddressDto, CreateIpAddressDto, UpdateIpAddressDto
```

NetBox's write endpoints generally accept/return the same shape (no separate list-item schema per resource) — decide per-resource whether `SiteListItemDto` is actually distinct from `SiteDetailDto`; do not create a duplicate DTO for schemas verified to be identical. Confirm actual NetBox response shape against the running SUT's OpenAPI schema (`/api/schema/`) during implementation, not from memory.

### DTO Builders (`API/Factories/`)

`record`-based DTOs + `BaseBuilder<T>`-derived builders, one per create-DTO:

```text
CreateSiteDtoBuilder : BaseBuilder<CreateSiteDto>
CreateDeviceDtoBuilder : BaseBuilder<CreateDeviceDto>
CreatePrefixDtoBuilder : BaseBuilder<CreatePrefixDto>
CreateIpAddressDtoBuilder : BaseBuilder<CreateIpAddressDto>
```

Defaults must satisfy the attachment's naming convention `auto-{feature}-{guid}` for `Name`/derived `Slug`, `Status = "active"`, `Description = "automation test data"`. Slug must be NetBox-slug-legal (lowercase, hyphens) — derive deterministically from `Name`, do not generate slug and name independently (risk of mismatch/collision).

## Database Layer

New `Database/NetBox/Queries/` with **read-only** repositories, following the prior resource-query pattern:

```text
SitesDatabaseRepository   → SELECT id, name, slug, status FROM dcim_site WHERE slug = @Slug
DevicesDatabaseRepository → SELECT id, name, site_id, status FROM dcim_device WHERE name = @Name
IpamDatabaseRepository    → prefix/ip-address lookups by CIDR or id
```

Row DTOs in `Database/NetBox/DTO/` (e.g. `SiteRowDto`), aliased columns matching C# member names per `.agents/rules/test-automation.md` (`snake_case` → PascalCase alias in SQL). Use `DapperActions.Query<T>`/`QueryAll<T>` against `PostgreSqlConnectionPool.Instance.GetConnection("netbox")` — never open ad-hoc connections in tests. Parameterize every query; no string concatenation.

## Browser Layer

New page objects in `UI/Pages/NetBox/` (folder is illustrative; match existing `UI/Pages` layout) deriving from `BaseUIView`/`BaseUIComponent`:

```text
NetBoxLoginPage     → username/password fields, sign-in button, PageReadyLocator on the dashboard heading
SitesListPage       → navigate, "Add" action → SiteEditPage; row-level "Delete" action → confirmation dialog
SiteEditPage        → CreateSiteAsync(name, slug, status, description) / UpdateAsync(status, description) → returns SiteDetailsPage (NetBox redirects to the detail view on successful save)
SiteDetailsPage     → read displayed fields for assertions; DeleteAsync() → confirms the delete dialog, returns SitesListPage
DevicesListPage     → navigate, "Add" action → DeviceEditPage
DeviceEditPage      → CreateDeviceAsync(name, site, deviceType, role, status) → returns DeviceDetailsPage
DeviceDetailsPage   → read displayed fields for assertions
PrefixesPage        → navigate, "Add" action → PrefixEditPage
PrefixEditPage      → CreatePrefixAsync(prefix, status, description) → returns PrefixDetailsPage (or PrefixesPage, confirm against the running SUT)
PrefixDetailsPage   → read displayed fields for assertions
IpAddressesPage     → not needed for this increment (no IP Address UI test planned)
```

This increment plans **5 UI tests** total (see the Definition of Done in
`00-overview.md`): Site create, Site update, Site delete (`02-site-management-spec.md`),
Device create (`03-device-management-spec.md`), and Prefix create
(`04-ipam-spec.md`). This is intentionally broader than the source
attachment's "one focused UI CRUD workflow" recommendation — see the
Decisions table in `00-overview.md` for the rationale.

Use semantic locators (`GetByRole`, `GetByLabel`) per `.agents/rules/test-automation.md` locator hierarchy. No raw selectors inside tests — actions only (`await sitesPage.CreateSiteAsync(site)`).

## UI Session Reuse

`NetBoxSession` (see the API Token Provisioning section above) covers API auth only. UI login uses `NetBoxLoginPage` directly — no MSAL bridging needed, and it is a separate concern from `NetBoxSession` (do not let the page object reach into the API session or vice versa). Default recommendation: shared Playwright `storageState` captured once in `OneTimeSetUp`, consistent with `PlaywrightBrowserFactory`'s existing `storageState` support noted in `docs/UI_TESTING_ARCHITECTURE.md`. Confirm this choice (vs per-test login) in the tracker once Phase 1 implementation starts.

## Allure Evidence

Reuse `ApiLoggingInterceptor` (already attaches request/response) — no new plumbing needed for API tests. For UI: reuse `AllureExtensions.CaptureScreenshotAsync`/`CaptureBrowserLogsAsync` already wired into `UiTestBase` on failure. For DB: no existing Allure DB-attachment helper — add one only if the Site UI cross-layer test needs to show the DB row; otherwise log via NLog and let the assertion failure carry the message. Confirm no secrets (token, password) ever reach an Allure attachment — sanitizer covers API; UI screenshots must avoid rendering the token (NetBox UI does not display the raw API token after creation, so this is low-risk, but keep it in mind if any admin token page is visited).

## Test Data Builders / Static Data

`TestData/NetBox/` (new): well-known constants (e.g. default `Status = "active"`), no fixed Site names/slugs (parallel-safety requirement) — every builder must generate a unique identifier.

## Cleanup

Register cleanup via `ScenarioCleanupActions.AddCleanUpAction(Func<Task>)` immediately after each create call, deleting in dependency order: IP Address → Prefix → Device → Device Type/Manufacturer/Role → Site. Cleanup actions must tolerate 404 (already deleted) without failing the test.

## Open Items for Implementation Time (not blocking spec approval)

1. Confirm whether NetBox's PATCH endpoints require `Content-Type: application/json` explicitly (RestSharp default should suffice — verify against a real call, not assumption).
2. Confirm real DTO field casing from `/api/schema/` (NetBox's REST API uses `snake_case` JSON field names) before finalizing `[JsonPropertyName]` values — do not assume camelCase. This gates tracker task 1.9 (see the DTOs section above).
