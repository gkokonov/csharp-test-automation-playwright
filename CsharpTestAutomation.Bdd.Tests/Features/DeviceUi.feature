@NetBox @Device @UI
Feature: Device management in the web application
    Administrators can create Devices and see their saved details.

    @DeviceUi01
    Scenario: Creating a Device in the web application saves its details
        Given an authenticated administrator
        And owned Device prerequisites and unique valid Device details
        When the Device is created in the web application
        Then the requested Device details are displayed and retrievable
        And the Device details are saved
