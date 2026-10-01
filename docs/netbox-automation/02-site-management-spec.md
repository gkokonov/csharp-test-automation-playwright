# 02 — Site Management Spec

First feature. Establishes conventions reused by Device (03) and IPAM (04).

## API Tests (`Tests/API/SitesApiTests.cs`)

Fixture: `[AllureSuite("API")] [AllureFeature("Site Management")]`. Derive from
plain `[TestFixture]` (or `ApiTestBase` if `RequireDbData` is needed — unlikely
for pure create/read/update/delete). Build `SitesApiClient` via
`IRestClientFactory` in `[SetUp]`; dispose in `[TearDown]`.

| Test | Steps | Key Assertions |
| --- | --- | --- |
| `CreateSite_ShouldReturnCreatedSite` | POST via `CreateSiteDtoBuilder` | `201 Created`; response `Name`/`Slug`/`Status` match request; `Id` non-empty; register cleanup |
| `CreateSite_ShouldPersistSiteInDatabase` | Create via API, then `GET /dcim/sites/{id}/` | GET response equivalent to POST response (`BeEquivalentTo`, excluding volatile fields e.g. `last_updated`) |
| `GetSite_ShouldReturnMatchingSiteBySlug` | Create, then `GET /dcim/sites/?slug={slug}` | Exactly one result; matches created site |
| `UpdateSite_ShouldChangeStatus` | Create, PATCH `status` (and `name`, `description` where applicable) | `200 OK`; GET reflects updated fields; unrelated fields unchanged |
| `DeleteSite_ShouldRemoveSite` | Create, DELETE, then GET by id | DELETE returns `204`; subsequent GET returns `404` |

Cleanup: register the delete call right after create in every test (idempotent — swallow `404` on cleanup).

## UI Tests (`Tests/UI/NetBox/SiteManagementUiTests.cs`)

Three focused scenarios — create, update, delete — giving Site feature full
UI CRUD coverage rather than the single create-only flow the source
attachment recommends (see `00-overview.md` Decisions for the rationale).
Fixture: `[AllureSuite("UI")] [AllureFeature("Site Management")]`, derived
from `NetBoxUiTestBase` (which wires the shared authenticated storage state
and, via `UiTestBase`, applies `[Category("UI")]` itself — do not repeat it
on the fixture).

### `CreateSite_ShouldPersistAcrossLayers`

```text
Given a logged-in NetBox administrator
And a unique Site test object (built via CreateSiteDtoBuilder, but NOT sent through the API)
When the user creates the Site through the UI (SitesListPage → SiteEditPage.CreateSiteAsync, which navigates to SiteDetailsPage on success — NetBox redirects to the detail view after a successful save)
Then the Site is displayed successfully (assert on SiteDetailsPage fields)
And the Site can be retrieved through the REST API (GET /dcim/sites/?slug=...)
And the Site exists in PostgreSQL (SitesDatabaseRepository)
```

### `UpdateSite_ShouldReflectChangedStatusAcrossLayers`

```text
Given a logged-in NetBox administrator
And a Site created via the API (SitesApiClient.CreateSiteAsync — setup only, not under test)
When the user edits the Site's status and description through the UI (SiteDetailsPage → SiteEditPage.UpdateAsync)
Then the updated values are displayed successfully on SiteDetailsPage
And the REST API reflects the updated status/description (GET /dcim/sites/{id}/)
And PostgreSQL reflects the updated status (SitesDatabaseRepository)
```

### `DeleteSite_ShouldRemoveSiteAcrossLayers`

```text
Given a logged-in NetBox administrator
And a Site created via the API (setup only, not under test)
When the user deletes the Site through the UI (SiteDetailsPage.DeleteAsync, confirming the dialog)
Then the Site no longer appears on SitesListPage
And the REST API returns 404 for that Site id
And PostgreSQL no longer has a row for that slug
```

- Fields: `Name`, `Slug`, `Status`, `Description`.
- Cross-layer assertions: UI visibility (`Expect(...).ToBeVisibleAsync()`), API returns exactly one match (or 404 post-delete), DB row matches `Name`/`Slug`/`Status` only (per "do not assert every internal database column").
- `CreateSite_ShouldPersistAcrossLayers` cleans up via the `SitesApiClient` (API delete), registered once the Site's id is known (fetch id via the slug-filtered GET immediately after UI creation, since the UI flow does not directly expose the created id). `UpdateSite_...` also registers API cleanup at setup time (it did not delete the Site itself). `DeleteSite_...` needs no cleanup — the test's own action removes the record; only assert, don't re-delete.

## Definition of Done (Site feature)

- All 5 API tests and all 3 UI tests pass 3 consecutive headless runs.
- No fixed Site name/slug anywhere (parallel-safety).
- Allure shows Epic `NetBox` / Feature `Site Management` / Story per test.
