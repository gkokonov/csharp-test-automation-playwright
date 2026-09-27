# 04 — Prefix and IP Address (IPAM) Spec

Implemented after Device automation. Primarily API-only, with one UI test
for Prefix create (this increment's 5th and final planned UI test; see
`00-overview.md` Decisions) — IP Address has no UI test this increment.

## Prefix Tests (`Tests/API/PrefixesApiTests.cs`)

Use dynamically generated RFC1918 ranges to avoid collisions (e.g. random
second/third octet under `10.0.0.0/8`), not the literal `10.10.0.0/24` example
verbatim across parallel runs.

| Test | Notes |
| --- | --- |
| `CreatePrefix_ShouldReturnCreatedPrefix` | POST `/ipam/prefixes/`; assert `prefix`, `status` |
| `GetPrefix_ShouldReturnMatchingPrefix` | `GET /ipam/prefixes/?prefix={cidr}` |
| `UpdatePrefix_ShouldChangeStatus` | PATCH `status`/`description` |
| `DeletePrefix_ShouldRemovePrefix` | Delete + verify 404 |

## IP Address Tests (`Tests/API/IpAddressesApiTests.cs`)

Create a Prefix first (as a prerequisite, cleaned up after), then an address
within its range (e.g. `{prefix-base}.10/{mask}`).

| Test | Notes |
| --- | --- |
| `CreateIpAddress_ShouldReturnCreatedAddress` | POST `/ipam/ip-addresses/` |
| `GetIpAddress_ShouldReturnMatchingAddress` | `GET /ipam/ip-addresses/?address={cidr}` |
| `UpdateIpAddress_ShouldChangeStatusOrRole` | PATCH `status` (e.g. `active` → `reserved`) — validates NetBox-specific IPAM status values, not generic CRUD |
| `DeleteIpAddress_ShouldRemoveAddress` | Delete + verify 404 |
| `CreateIpAddress_ShouldRejectAddressOutsideConfiguredPrefix` *(stretch — only if time permits)* | Documents NetBox-specific validation behavior; optional for this increment |

## Cleanup Order

IP Address → Prefix (address depends on prefix only loosely in NetBox — no FK
enforcement — but still delete address first to avoid dangling test data).

## UI Test (`Tests/UI/IpamManagementUiTests.cs`)

One scenario — `CreatePrefix_ShouldPersistAcrossLayers`:

```text
Given a logged-in NetBox administrator
And a unique, dynamically generated RFC1918 prefix (not sent through the API)
When the user creates the Prefix through the UI (PrefixesPage → PrefixEditPage.CreatePrefixAsync)
Then the Prefix is displayed successfully (assert on PrefixDetailsPage fields)
And the Prefix can be retrieved through the REST API (GET /ipam/prefixes/?prefix=...)
And the Prefix exists in PostgreSQL (IpamDatabaseRepository)
```

Cleanup via the `IpamApiClient` (API delete), registered once the Prefix's id
is known (fetch id via the prefix-filtered GET immediately after UI
creation). No IP Address UI test is planned this increment.

## Definition of Done (IPAM feature)

- All Prefix and IP Address API tests, and the Prefix UI test, pass 3 consecutive headless runs.
- No fixed/reused CIDR ranges across tests (parallel-safety) — verify by running the fixture with NUnit parallel execution enabled at least once.
- At least one assertion validates NetBox-specific IPAM behavior (e.g. status transition) rather than only generic CRUD, per the source instructions.
