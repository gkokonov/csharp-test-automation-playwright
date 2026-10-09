@NetBox @Device @API
Feature: Device management through the service
    Administrators can manage Devices and retrieve their saved details.

    @DeviceApi01
    Scenario: Creating a Device returns its requested details
        Given owned Device prerequisites and unique valid Device details
        When the Device is created through the service
        Then the requested Device details are returned
        And the Device details are saved

    @DeviceApi02
    Scenario: Finding a Device by its exact name returns only that Device
        Given an owned active Device
        When the Device is found by its exact name
        Then only the expected Device is returned
        And the Device details are saved

    @DeviceApi03
    Scenario: Updating a Device retains its original Site
        Given owned Device prerequisites and unique valid Device details
        And an owned active Device at the original Site
        And valid Device status and description changes
        When the Device changes are submitted through the service
        Then the service saves the Device changes and retains its other details
        And the Device details are saved

    @DeviceApi04
    Scenario: Moving a Device saves its new Site and changed details
        Given owned Device prerequisites and unique valid Device details
        And another owned Device Site
        And an owned active Device at the original Site
        And valid Device changes that move it to the other Site
        When the Device changes are submitted through the service
        Then the service saves the Device changes and retains its other details
        And the Device details are saved

    @DeviceApi05
    Scenario: Deleting a Device makes it unavailable
        Given an owned active Device
        When the Device is deleted through the service
        Then the Device is unavailable through the service
        And the Device is no longer saved

    @DeviceApi06
    Scenario: Filtering Devices by Site includes only matching Devices
        Given two owned Devices at one Site and another Device at a different Site
        When Devices are found by the original Site
        Then only the Devices at the original Site are returned
        And all owned Device details are saved

    @DeviceApi07
    Scenario: Cleanup removes prerequisites after interruption before Device creation
        Given owned Device prerequisites and unique valid Device details
        When the Device scenario is interrupted and cleanup runs
        Then all owned Device records and prerequisites are unavailable
        And the Device is no longer saved

    @DeviceApi08
    Scenario: Cleanup removes the Device and prerequisites after interruption
        Given an owned active Device
        When the Device scenario is interrupted and cleanup runs
        Then all owned Device records and prerequisites are unavailable
        And the Device is no longer saved
