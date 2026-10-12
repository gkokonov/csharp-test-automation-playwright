# IPAM NUnit to BDD parity

The BDD project owns its application layers and references only the framework.
The original NUnit fixtures remain unchanged. Each row is one reported scenario.

| NUnit fixture and method | BDD ID | Required checks |
| --- | --- | --- |
| `PrefixesApiTests.Verify_CreatedPrefixReturnedAndPersisted_When_PrefixIsCreated` | `PrefixApi01` | Created status, non-null positive ID, requested fields, full retrieval equivalence, all represented database fields. |
| `PrefixesApiTests.Verify_MatchingPrefixReturned_When_PrefixIsFilteredByCidr` | `PrefixApi02` | Successful exact CIDR filter, count one, single full representation. |
| `PrefixesApiTests.Verify_ReservedStatusPersisted_When_ActivePrefixIsUpdated` | `PrefixApi03` | Active prerequisite, successful update, reserved value and label, complete unchanged-field equivalence, retrieval equivalence, database fields. |
| `PrefixesApiTests.Verify_PrefixRemovedAcrossLayers_When_PrefixIsDeleted` | `PrefixApi04` | Successful delete, unavailable retrieval, absent database row. |
| `IpAddressesApiTests.Verify_CreatedIpAddressReturnedAndPersisted_When_IpAddressIsCreated` | `IpAddressApi01` | Owned prerequisite Prefix, address in its /24, Created status, positive ID, requested fields, retrieval equivalence, database fields with mask. |
| `IpAddressesApiTests.Verify_MatchingIpAddressReturned_When_IpAddressIsFilteredByCidr` | `IpAddressApi02` | Owned prerequisite Prefix/address, successful exact address filter, count one, single full representation. |
| `IpAddressesApiTests.Verify_ReservedStatusPersisted_When_ActiveIpAddressIsUpdated` | `IpAddressApi03` | Owned prerequisite Prefix, active address, successful update, reserved value and label, complete unchanged-field equivalence, retrieval equivalence, database fields. |
| `IpAddressesApiTests.Verify_IpAddressRemovedAcrossLayers_When_IpAddressIsDeleted` | `IpAddressApi04` | Owned prerequisite Prefix/address, successful delete, unavailable retrieval, absent database row. |
| `IpamManagementUiTests.Verify_PrefixPersistedAcrossLayers_When_PrefixIsCreatedThroughUi` | `PrefixUi01` | Web-only creation, heading/status/description, successful unique description and CIDR lookup, count one, positive ID, requested response fields, database fields. |

API cleanup registers positive IDs before response assertions. Address cleanup
runs before prerequisite Prefix cleanup. Deletion accepts only success or an
already absent record. Web cleanup registers a lookup before form submission;
both CIDR and unique description filter on the server and are checked again
before deletion. Identified IDs register before lookup assertions. Shared hooks
retain clients through cleanup and release each scenario's resources afterward.

CIDRs are random RFC1918 /24 values reserved under a lock for the process
lifetime. Separate test processes do not share reservations. Scenario state is
fresh per scenario. Story, severity, and owner match the original NUnit metadata.

Validation is recorded in [the tracker](TRACKER.md#bdd-ipam-port-2026-10-12).
