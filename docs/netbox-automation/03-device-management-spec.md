# 03 — Device Management Spec

Implemented after Site automation is stable. Extends Site conventions.

Implementation uses the repository's `Verify_[ExpectedBehavior]_When_[StateUnderTest]`
test names. The scenario names below identify the planned behavior. Prerequisite
builders already exist from Phase 1 and are reused through `DeviceSteps`
in `Steps/API/NetBox/`,
which borrows fixture-registered clients and the inherited cleanup stack.

## Prerequisites (create via API only, never via UI)

- `Manufacturer` (`ManufacturersApiClient.CreateManufacturerAsync`)
- `DeviceType` (`DeviceTypesApiClient.CreateDeviceTypeAsync`, references Manufacturer)
- `DeviceRole` (`DeviceRolesApiClient.CreateDeviceRoleAsync`)
- `Site` (`SitesApiClient.CreateSiteAsync`, reused from Site feature)

Each prerequisite gets its own builder (`CreateManufacturerDtoBuilder`, `CreateDeviceTypeDtoBuilder`, `CreateDeviceRoleDtoBuilder`) with unique `auto-{feature}-{guid}` names. Register cleanup immediately in creation order. LIFO cleanup executes deepest dependencies first: Device → DeviceType → Manufacturer, with DeviceRole and Site deleted after their Devices.

Create every destination Site before its Device. Register cleanup in creation
order so Devices are deleted before Sites, types, roles, and manufacturers.
Delete cleanup accepts `204` and `404`; other failures must remain visible.
For shared ownership and step conventions, read the
[test rules](../../.agents/rules/test-automation.md); API and UI scenarios also
follow their [API](../../.agents/rules/api-testing.md) and
[UI](../../.agents/rules/ui-testing.md) rules, respectively.

## API Tests (`Tests/API/DevicesApiTests.cs`)

| Test | Notes |
| --- | --- |
| `CreateDevice_ShouldReturnCreatedDevice` | POST `/dcim/devices/` with Site/DeviceType/DeviceRole ids from prerequisites |
| `GetDevice_ShouldReturnMatchingDeviceByName` | `GET /dcim/devices/?name={name}` (NetBox device names are typically unique per site — confirm filter semantics against the running SUT) |
| `UpdateDevice_ShouldChangeStatusAndSite` | Two cases: PATCH status/description without a site field, and PATCH with a second owned Site. Create the second Site before the Device so cleanup can delete the Device first. |
| `DeleteDevice_ShouldRemoveDevice` | Delete + verify 404 |
| `FilterDevices_ShouldReturnOnlyMatchingSite` | `GET /dcim/devices/?site_id={id}` returns only devices for that site — validates filter/search coverage called out in the source instructions |

## Cross-Layer Validation

Mirror the Site pattern: DB read via new `DevicesDatabaseRepository` (`SELECT id, name, site_id, status FROM dcim_device WHERE name = @Name`).

The implemented DB projection also includes `device_type_id`, `role_id`, and
`description` so assertions compare every persisted field represented by the
Device API DTO. Derived URLs and display labels are excluded from DB comparison.

## UI Test (`Tests/UI/NetBox/DeviceManagementUiTests.cs`)

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

Cleanup: register lookup-based Device cleanup before UI submission (find by its
unique name through the API and restrict deletion to its owned prerequisite
Site). Register prerequisite deletes immediately in creation order; LIFO cleanup
then deletes the Device before all prerequisites even if UI assertions fail.
No other Device UI scenarios (update/delete) are
planned this increment — add only if time permits.

## Definition of Done (Device feature)

- All prerequisite and Device API tests, and the Device UI test, pass 3 consecutive headless runs.
- No test creates Manufacturer/DeviceType/DeviceRole/Site through the UI (the Device UI test creates only the Device itself through the UI).
- Cleanup order verified (delete Device before its DeviceType/Site/etc.) even when a test fails mid-way (use `ScenarioCleanupActions` LIFO ordering — register in creation order so cleanup pops in reverse).
