@NetBox @Site @API @allure.label.epic:NetBox @allure.label.suite:API @allure.label.owner:AutomationTeam
Feature: Site management through the service
    Administrators can manage Sites and retrieve their saved details.

    @SiteApi01 @critical
    Scenario: Creating a Site returns its requested details
        Given unique valid Site details
        When the Site is created through the service
        Then the requested Site details are returned
        And the Site details are saved

    @SiteApi02 @critical
    Scenario: A created Site can be retrieved with its original details
        Given an owned active Site
        When the Site is retrieved through the service
        Then the original Site details are returned
        And the Site details are saved

    @SiteApi03 @normal
    Scenario: Finding a Site by its slug returns only that Site
        Given an owned active Site
        When the Site is found by its slug
        Then only the expected Site is returned

    @SiteApi04 @normal
    Scenario: Updating a Site changes its status and description
        Given an owned active Site
        And a Site status change to "planned" and a new description
        When the Site changes are submitted through the service
        Then the service saves the changes and retains the other Site details
        And the Site details are saved

    @SiteApi05 @critical
    Scenario: Deleting a Site makes it unavailable
        Given an owned active Site
        When the Site is deleted through the service
        Then the Site is unavailable through the service
        And the Site is no longer saved
