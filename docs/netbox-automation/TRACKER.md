# NetBox Automation Tracker

All planned implementation phases are complete. Detailed behavior belongs in
the [feature specifications](00-overview.md).

| Phase | Scope | Status | Key evidence |
| --- | --- | --- | --- |
| 0 | Remove legacy application assets | DONE | Obsolete application clients, tests, and configuration were removed. |
| 1 | NetBox configuration, clients, DTOs, database access, and shared UI pages | DONE | API and database contracts were checked against NetBox 4.7. |
| 2 | Site API tests | DONE | Five cases passed on three consecutive live runs. |
| 3 | Site UI tests | DONE | Three cases passed on three consecutive live runs. |
| 4 | Device API and UI automation | DONE | Device coverage passed in three consecutive 17/17 Site/Device runs. |
| 5 | Prefix and IP Address API/UI automation | DONE | Nine IPAM cases and the 26-case NetBox regression passed on three consecutive live runs. |
| BDD IPAM | Port eight API and one UI case to standalone BDD scenarios | DONE | Nine scenarios passed on three consecutive runs; full BDD NetBox regression passed 26/26. |

## Current validation evidence

- Latest BDD IPAM runs: `artifacts/bdd-ipam/ipam-final-{1,2,3}.trx`.
- Latest full BDD NetBox regression: `artifacts/bdd-ipam/netbox-run-2.trx`.
- NUnit IPAM and full NetBox runs: `artifacts/netbox-phase5/netbox-final-{1,2,3}.trx`.
- Debug and Release builds, scoped formatting/analyzer checks, and the latest
  independent review passed for the BDD IPAM port.

## Operational notes

- IPAM `/24` reservations are process-local. Separate test processes do not
  share the reservation set.
- Cleanup registers owned records at creation time and removes dependents before
  prerequisites.
- Exactly five UI tests are in scope: three Site, one Device, and one Prefix.
- Historical ignored Allure results from before credential redaction may still
  contain authorization headers. New API logs and Allure attachments redact
  credentials.
