@NetBox @Ipam @Prefix @UI
Feature: Prefix management in the web application
    Administrators can create Prefixes and retrieve their saved details.

    @PrefixUi01
    Scenario: Creating a Prefix displays and saves its requested details
        Given unique valid Prefix details for the web application
        When the Prefix is created in the web application
        Then the requested Prefix details are displayed and retrievable
        And the Prefix details are saved
