@NetBox @Site @API
Feature: Site management through the service
    Administrators can manage Sites and retrieve their saved details.

    @SiteApi01
    Scenario: Creating a Site returns its requested details
        Given unique valid Site details
        When the Site is created through the service
        Then the requested Site details are returned
        And the Site details are saved

    @SiteApi02
    Scenario: A created Site can be retrieved with its original details
        Given an owned active Site
        When the Site is retrieved through the service
        Then the original Site details are returned
        And the Site details are saved

    @SiteApi03
    Scenario: Finding a Site by its slug returns only that Site
        Given an owned active Site
        When the Site is found by its slug
        Then only the expected Site is returned

    @SiteApi04
    Scenario: Updating a Site changes its status and description
        Given an owned active Site
        And valid changes to its status and description
        When the Site changes are submitted through the service
        Then the service saves the changes and retains the other Site details
        And the Site details are saved

    @SiteApi05
    Scenario: Deleting a Site makes it unavailable
        Given an owned active Site
        When the Site is deleted through the service
        Then the Site is unavailable through the service
        And the Site is no longer saved
