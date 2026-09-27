# NetBox Automation — Overview

Spec set for the first NetBox SUT automation increment. This is a **planning
document set only** — no implementation code is part of this change. Use the
tracker (`TRACKER.md`) to follow implementation once work starts.

## Decisions (confirmed by user, 2026-09-27)

| Question | Decision | Rationale |
| --- | --- | --- |
| Project structure | Reuse the existing three projects (`CsharpTestAutomation.Framework`, `CsharpTestAutomation.Tests`, `CsharpTestAutomation.Framework.Test`) instead of the attachment's suggested `NetBox.*.Tests` / `Test.Infrastructure` layout | The repo already has a working, convention-documented framework/test split (see `AGENTS.md`, `docs/API_TESTING_ARCHITECTURE.md`, `docs/UI_TESTING_ARCHITECTURE.md`). Introducing parallel projects would duplicate infrastructure the attachment itself says to avoid. |
| Obsolete CPF tests | Removed, but good patterns preserved first. Before deletion, extract any CPF-based examples currently embedded in AI harness instructions (`docs/API_TESTING_ARCHITECTURE.md`, `docs/UI_TESTING_ARCHITECTURE.md`, `.agents/rules/test-automation.md`) into generic or NetBox-based examples. After deletion, re-evaluate those instruction files end-to-end to confirm no reference points at a file/class that no longer exists | CPF app no longer exists; its API clients, DTOs, page objects, and DB queries are dead code once NetBox lands. The instruction files currently use CPF (`CpfAppApiClient`, `GetCpfsTests`, `CreateCpfDtoBuilder`, `Database/CPF/...`) as their worked examples — deleting the code without updating the docs would leave the harness instructions pointing at nonexistent files. |
| Spec storage | `docs/netbox-automation/*.md` | Keeps NetBox planning separate from architecture references in `docs/`, mirrors `docs/known-defects/` precedent for topic subfolders. |
| NetBox auth | New, NetBox-specific lightweight auth flow (API token provisioning + plain-credential UI login) — does **not** reuse `BootstrapSession` / `UiAuthenticationBootstrapper`. Design it using the **software-design-principles** skill (SRP/DIP/composition) rather than ad hoc | Those two classes are MSAL/Entra-ID specific (interactive federated login, JWT-from-UI bridging). NetBox uses local `admin`/`admin` credentials and a directly provisioned REST token — a materially simpler flow that would only be contorted by forcing it through the MSAL-shaped abstraction. Applying the design-principles skill keeps token provisioning, token application, and token caching as separate, independently testable collaborators instead of one god class. |
| Secrets | `CsharpTestAutomation.Tests/appsettings.local.json` (gitignored), same file already used for CPF secrets. It **mirrors the full key set** of the committed `appsettings.json` — only secret values (`Password`, tokens) differ between the two; every non-secret key must exist in both files | Existing, already-ignored mechanism; keeps `NETBOX_*` env vars as the CI override path per the attachment. Mirroring the full structure means a developer only ever edits `appsettings.local.json` and never has to guess which keys it's missing. |
| UI test coverage | **5 UI tests** total: Site create/update/delete (`02-site-management-spec.md`), Device create (`03-device-management-spec.md`), Prefix create (`04-ipam-spec.md`) | User explicitly requested at least 3-5 UI tests, superseding the source attachment's "implement one focused UI CRUD workflow" recommendation. Spreading them across create/update/delete on Site (the most-exercised feature) plus one create flow each for Device and Prefix demonstrates the page-object pattern extends cleanly to every feature, without building a full UI CRUD suite for every resource. |

All open questions from the initial review have been answered. Re-open only if new information contradicts a decision above.

## Scope

1. Infrastructure: configuration, typed API client(s), DB repository, auth, Allure evidence, test-data builders.
2. Site management: API CRUD + 3 UI flows (create, update, delete) with cross-layer (UI/API/DB) validation.
3. Device management: extends Site conventions; API CRUD + 1 UI create flow.
4. Prefix / IP Address (IPAM) management: extends Device conventions; API CRUD + 1 UI create flow (Prefix only).

## Spec Documents

| File | Covers |
| --- | --- |
| [01-infrastructure-spec.md](01-infrastructure-spec.md) | Configuration, `NetBoxApiClient`, DB repository, auth/session, Allure, builders, Phase 0 CPF pattern-extraction and removal |
| [02-site-management-spec.md](02-site-management-spec.md) | Site API CRUD tests + 3 Site UI flows (create/update/delete) + cross-layer validation |
| [03-device-management-spec.md](03-device-management-spec.md) | Device prerequisites (manufacturer/device type/role) + Device API tests + 1 Device UI create test |
| [04-ipam-spec.md](04-ipam-spec.md) | Prefix and IP Address API tests + 1 Prefix UI create test |
| [TRACKER.md](TRACKER.md) | Implementation checklist/status, updated as work proceeds |

## Out of Scope (this increment)

- CI pipeline wiring beyond what already exists for `dotnet test`.
- NetBox plugins, custom fields, or webhooks.
- Non-Site/Device/IPAM NetBox modules (circuits, virtualization, tenancy, etc.).
- Full negative/permission-matrix testing — first pass covers CRUD plus the 5 planned UI flows described above; the source instructions' "one focused UI CRUD workflow" recommendation was widened per user request (see Decisions).

## Definition of Done for the Whole Increment

Mirrors `.agents/rules/test-automation.md` Definition of Done, plus the attachment's own Definition of Done section (solution builds under .NET 10, Site/Device/IPAM API CRUD passes, all 5 planned UI flows pass, Postgres validation works, cleanup is reliable and order-independent, test data is unique, Allure has meaningful diagnostics, no secrets logged, no duplicate infrastructure introduced).

## References (SUT Documentation)

Consult these during implementation for endpoint shapes, field casing, UI
structure, and NetBox-specific behavior — do not guess when the answer is
one lookup away:

| Resource | URL | Use for |
| --- | --- | --- |
| NetBox source repository | <https://github.com/netbox-community/netbox> | Ground truth for model field names/choices (e.g. `dcim/models/sites.py`), migrations (DB column names), and REST serializer shapes when the running SUT's `/api/schema/` is ambiguous. |
| NetBox Labs official docs | <https://netboxlabs.com/docs/netbox/> | User-facing UI workflows (menu paths, form fields) for page objects, REST API usage guide, and IPAM/DCIM concept definitions (status values, prefix/IP relationships). |
| Local running instance | `http://localhost:8000/` (UI), `http://localhost:8000/api/` (REST), `http://localhost:8000/api/schema/swagger-ui/` (Swagger) | Primary source of truth — confirm exact request/response field casing and UI locators against this instance rather than assuming from docs or source, since versions can drift. |

### Local PostgreSQL Connection

For ad hoc inspection only (`psql`, a DB client) — tests must go through `PostgreSqlConnectionPool` / `DapperActions`, never a direct connection string in code:

```text
Host:     localhost
Port:     5432
Database: netbox
Username: netbox
Password: J5brHrAXFLQSif0K
```

This is the same value that goes into `CsharpTestAutomation.Tests/appsettings.local.json` under `DbSettings.Database[].Password` (see `01-infrastructure-spec.md`). It is a local Docker Compose dev credential, not a production secret, but it must still never appear in the committed `appsettings.json`, logs, or Allure attachments.
