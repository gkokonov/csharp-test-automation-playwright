@NetBox @Ipam @IpAddress @API
Feature: IP Address management through the service
    Administrators can manage IP Addresses and retrieve their saved details.

    @IpAddressApi01
    Scenario: Creating an IP Address returns its requested details
        Given unique valid IP Address details
        When the IP Address is created through the service
        Then the requested IP Address details are returned and retrievable
        And the IP Address details are saved

    @IpAddressApi02
    Scenario: Finding an IP Address by its address returns only that IP Address
        Given an owned active IP Address
        When the IP Address is found by its address
        Then only the expected IP Address is returned

    @IpAddressApi03
    Scenario: An active IP Address can be reserved without changing its CIDR
        Given an owned active IP Address
        And valid changes to the IP Address status and description
        When the IP Address changes are submitted through the service
        Then the service reserves the IP Address and retains its other details
        And the IP Address details are saved

    @IpAddressApi04
    Scenario: Deleting an IP Address removes its saved details
        Given an owned active IP Address
        When the IP Address is deleted through the service
        Then the IP Address is unavailable through the service
