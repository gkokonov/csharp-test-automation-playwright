# 02 — Site Management Spec

First feature. Establishes conventions reused by Device (03) and IPAM (04).

## API Tests (`Tests/API/SitesApiTests.cs`)

Fixture: `[AllureSuite("API")] [AllureFeature("Site Management")]`. Derive from
`ApiTestBase` (mandatory for every API test fixture — see
`.agents/rules/test-automation.md` "API Test Fixtures"). Build `SitesApiClient`
from the inherited `RestClientFactory`/`NetBoxAuthenticator` in an overridden
`OnSetUpAsync()`, and register it with `ApiTestBase.RegisterClient(...)` (not
`TestContainer` directly) so it is disposed after `ScenarioCleanupActions` runs.

| Test | Steps | Key Assertions |
| --- | --- | --- |
| `CreateSite_ShouldReturnCreatedSite` | POST via `CreateSiteDtoBuilder` | `201 Created`; response `Name`/`Slug`/`Status` match request; `Id` non-empty; register cleanup |
| `CreateSite_ShouldPersistSiteInDatabase` | Create via API, then `GET /dcim/sites/{id}/` | GET response equivalent to POST response (`BeEquivalentTo`, excluding volatile fields e.g. `last_updated`) |
| `GetSite_ShouldReturnMatchingSiteBySlug` | Create, then `GET /dcim/sites/?slug={slug}` | Exactly one result; matches created site |
| `UpdateSite_ShouldChangeStatus` | Create, PATCH `status` (and `name`, `description` where applicable) | `200 OK`; GET reflects updated fields; unrelated fields unchanged |
| `DeleteSite_ShouldRemoveSite` | Create, DELETE, then GET by id | DELETE returns `204`; subsequent GET returns `404` |

`SiteSteps` borrows the fixture's client and cleanup stack. It registers cleanup
as soon as a positive response id is available, before the fixture validates
the response. Cleanup accepts `204` and `404`; other failures remain visible.
API create and update success paths compare the represented persisted fields
with PostgreSQL. Delete verifies that the row is absent. Check the follow-up
GET status before comparing its payload.

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
And PostgreSQL reflects the updated status and description (SitesDatabaseRepository)
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
- Cross-layer assertions: UI visibility (`Expect(...).ToBeVisibleAsync()`), API returns exactly one match (or 404 post-delete), and DB row matches `Id`, `Name`, `Slug`, `Status`, and `Description`. URL, display text, and status labels are derived values.
- `CreateSite_ShouldPersistAcrossLayers` registers lookup-based cleanup by its unique slug before UI submission. It also registers id-based cleanup once the API lookup identifies the created Site. Update and delete scenarios register id-based cleanup during API setup, before response assertions, so early failures still remove their owned records. Cleanup tolerates `404` after a scenario deletes its Site.

## Definition of Done (Site feature)

- All 5 API tests and all 3 UI tests pass 3 consecutive headless runs.
- No fixed Site name/slug anywhere (parallel-safety).
- Allure shows Epic `NetBox` / Feature `Site Management` / Story per test.
