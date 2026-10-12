@NetBox @Ipam @Prefix @API
Feature: Prefix management through the service
    Administrators can manage Prefixes and retrieve their saved details.

    @PrefixApi01
    Scenario: Creating a Prefix returns its requested details
        Given unique valid Prefix details
        When the Prefix is created through the service
        Then the requested Prefix details are returned and retrievable
        And the Prefix details are saved

    @PrefixApi02
    Scenario: Finding a Prefix by its CIDR returns only that Prefix
        Given an owned active Prefix
        When the Prefix is found by its CIDR
        Then only the expected Prefix is returned

    @PrefixApi03
    Scenario: An active Prefix can be reserved without changing its CIDR
        Given an owned active Prefix
        And valid changes to the Prefix status and description
        When the Prefix changes are submitted through the service
        Then the service reserves the Prefix and retains its other details
        And the Prefix details are saved

    @PrefixApi04
    Scenario: Deleting a Prefix removes its saved details
        Given an owned active Prefix
        When the Prefix is deleted through the service
        Then the Prefix is unavailable through the service
