# 03 — Device Management Spec

Implemented after Site automation is stable. Extends Site conventions.

## Prerequisites (create via API only, never via UI)

- `Manufacturer` (`ManufacturersApiClient.CreateManufacturerAsync`)
- `DeviceType` (`DeviceTypesApiClient.CreateDeviceTypeAsync`, references Manufacturer)
- `DeviceRole` (`DeviceRolesApiClient.CreateDeviceRoleAsync`)
- `Site` (`SitesApiClient.CreateSiteAsync`, reused from Site feature)

Each prerequisite gets its own builder (`CreateManufacturerDtoBuilder`, `CreateDeviceTypeDtoBuilder`, `CreateDeviceRoleDtoBuilder`) with unique `auto-{feature}-{guid}` names. Register cleanup for each, deepest dependency first: Device → DeviceType → Manufacturer / DeviceRole / Site (Manufacturer, DeviceRole, Site have no further dependents among these four).

## API Tests (`Tests/API/DevicesApiTests.cs`)

| Test | Notes |
| --- | --- |
| `CreateDevice_ShouldReturnCreatedDevice` | POST `/dcim/devices/` with Site/DeviceType/DeviceRole ids from prerequisites |
| `GetDevice_ShouldReturnMatchingDeviceByName` | `GET /dcim/devices/?name={name}` (NetBox device names are typically unique per site — confirm filter semantics against the running SUT) |
| `UpdateDevice_ShouldChangeStatusAndSite` | PATCH `status`, optionally re-parent to a second Site created for this test |
| `DeleteDevice_ShouldRemoveDevice` | Delete + verify 404 |
| `FilterDevices_ShouldReturnOnlyMatchingSite` | `GET /dcim/devices/?site_id={id}` returns only devices for that site — validates filter/search coverage called out in the source instructions |

## Cross-Layer Validation

Mirror the Site pattern: DB read via new `DevicesDatabaseRepository` (`SELECT id, name, site_id, status FROM dcim_device WHERE name = @Name`).

## UI Test (`Tests/UI/DeviceManagementUiTests.cs`)

One scenario — `CreateDevice_ShouldPersistAcrossLayers` — extending the Site
UI pattern to Device (part of this increment's 3-5 planned UI tests; see
`00-overview.md` Decisions):

```text
Given a logged-in NetBox administrator
And Manufacturer, DeviceType, DeviceRole, and Site prerequisites created via the API (setup only, not under test)
When the user creates the Device through the UI (DevicesListPage → DeviceEditPage.CreateDeviceAsync, selecting the prerequisite Site/DeviceType/Role)
Then the Device is displayed successfully (assert on DeviceDetailsPage fields)
And the Device can be retrieved through the REST API (GET /dcim/devices/?name=...)
And the Device exists in PostgreSQL (DevicesDatabaseRepository)
```

Cleanup: register API deletes for the Device (fetched by name via the API
after UI creation) and all prerequisites, deepest dependency first, same as
the API tests above. No other Device UI scenarios (update/delete) are
planned this increment — add only if time permits.

## Definition of Done (Device feature)

- All prerequisite and Device API tests, and the Device UI test, pass 3 consecutive headless runs.
- No test creates Manufacturer/DeviceType/DeviceRole/Site through the UI (the Device UI test creates only the Device itself through the UI).
- Cleanup order verified (delete Device before its DeviceType/Site/etc.) even when a test fails mid-way (use `ScenarioCleanupActions` LIFO ordering — register in creation order so cleanup pops in reverse).
