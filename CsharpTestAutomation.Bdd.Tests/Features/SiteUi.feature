@NetBox @Site @UI @allure.label.epic:NetBox @allure.label.suite:UI @allure.label.owner:AutomationTeam
Feature: Site management in the web application
    Administrators can manage Sites in the application and see their saved details.

    Background:
        Given an authenticated administrator

    @SiteUi01 @critical
    Scenario: Creating a Site in the web application saves its details
        Given unique valid Site details
        When the Site is created in the web application
        Then the requested Site details are displayed and retrievable
        And the Site details are saved

    @SiteUi02 @normal
    Scenario: Editing a Site in the web application saves its changes
        Given an owned active Site
        And a Site status change to "planned" and a new description
        When the Site is edited in the web application
        Then the application displays and saves the changes and retains the other Site details
        And the Site details are saved

    @SiteUi03 @critical
    Scenario: Deleting a Site in the web application removes it
        Given an owned active Site
        When the Site is deleted in the web application
        Then the Site is absent from the application and service
        And the Site is no longer saved
