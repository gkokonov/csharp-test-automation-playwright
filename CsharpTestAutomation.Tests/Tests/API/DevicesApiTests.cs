using System.Net;
using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CsharpTestAutomation.Tests.Api.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Tests.Api.Dtos.Devices;
using CsharpTestAutomation.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Tests.Api.Factories;
using CsharpTestAutomation.Tests.Database.NetBox.DTO;
using CsharpTestAutomation.Tests.Database.NetBox.Queries;
using CsharpTestAutomation.Tests.Steps.Api.NetBox;
using RestSharp;

namespace CsharpTestAutomation.Tests.Tests.API;

[AllureSuite("API")]
[AllureEpic("NetBox")]
[AllureFeature("Device Management")]
public class DevicesApiTests : ApiTestBase
{
    private DevicesApiClient DevicesClient => GetClient<DevicesApiClient>();

    private DeviceSteps DeviceSteps => new(
        GetClient<ManufacturersApiClient>(), GetClient<DeviceTypesApiClient>(),
        GetClient<DeviceRolesApiClient>(), GetClient<SitesApiClient>(), DevicesClient, ScenarioCleanupActions);

    protected override async Task OnSetUpAsync()
    {
        await base.OnSetUpAsync();
        RegisterClient(new ManufacturersApiClient(RestClientFactory, NetBoxAuthenticator));
        RegisterClient(new DeviceTypesApiClient(RestClientFactory, NetBoxAuthenticator));
        RegisterClient(new DeviceRolesApiClient(RestClientFactory, NetBoxAuthenticator));
        RegisterClient(new SitesApiClient(RestClientFactory, NetBoxAuthenticator));
        RegisterClient(new DevicesApiClient(RestClientFactory, NetBoxAuthenticator));
    }

    [Test]
    [AllureStory("Creating a Device returns its requested fields and persists them in PostgreSQL")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    public async Task Verify_CreatedDeviceReturnedAndPersisted_When_DeviceIsCreated()
    {
        // Arrange
        DevicePrerequisites prerequisites = await DeviceSteps.CreatePrerequisitesAsync();
        CreateDeviceDto request = BuildDevice(prerequisites);

        // Act
        RestResponse<DeviceDetailDto> response = await DeviceSteps.CreateDeviceAsync(request);

        // Assert
        DeviceDetailDto created = RequireDevice(response, HttpStatusCode.Created);
        AssertRequestedDevice(created, request);
        AssertPersistedDevice(created);
    }

    [Test]
    [AllureStory("An exact Device name search returns the created Device")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public async Task Verify_MatchingDeviceReturned_When_DevicesAreSearchedByName()
    {
        // Arrange
        DevicePrerequisites prerequisites = await DeviceSteps.CreatePrerequisitesAsync();
        CreateDeviceDto request = BuildDevice(prerequisites);
        DeviceDetailDto created = RequireDevice(await DeviceSteps.CreateDeviceAsync(request), HttpStatusCode.Created);

        // Act
        RestResponse<PagedResultDto<DeviceDetailDto>> response = await DevicesClient.FindDevicesByNameAsync(request.Name);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Count.Should().Be(1);
        response.Data.Results.Should().ContainSingle().Which.Should().BeEquivalentTo(created);
        AssertPersistedDevice(created);
    }

    [TestCase(false, TestName = "Verify_StatusChangedAndSitePreserved_When_DeviceIsUpdated")]
    [TestCase(true, TestName = "Verify_StatusAndSiteChanged_When_DeviceIsReparented")]
    [AllureStory("PATCH changes Device status and description, and moves its site only when requested")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public async Task Verify_RequestedFieldsChanged_When_DeviceIsUpdated(bool changeSite)
    {
        // Arrange
        DevicePrerequisites prerequisites = await DeviceSteps.CreatePrerequisitesAsync();
        SiteDetailDto expectedSite = changeSite ? await DeviceSteps.CreateSiteAsync() : prerequisites.Site;
        CreateDeviceDto request = BuildDevice(prerequisites);
        DeviceDetailDto created = RequireDevice(await DeviceSteps.CreateDeviceAsync(request), HttpStatusCode.Created);
        UpdateDeviceDto update = new UpdateDeviceDtoBuilder().Default()
            .With(x => x.Site = changeSite ? expectedSite.Id : null).Build();

        // Act
        RestResponse<DeviceDetailDto> response = await DevicesClient.UpdateDeviceAsync(created.Id, update);

        // Assert
        DeviceDetailDto updated = RequireDevice(response, HttpStatusCode.OK);
        using (new AssertionScope())
        {
            updated.Should().BeEquivalentTo(created, options => options
                .Excluding(x => x.Site).Excluding(x => x.Status).Excluding(x => x.Description),
                "only Site, Status and Description were requested to change");
            updated.Site.Should().BeEquivalentTo(new { expectedSite.Id, expectedSite.Url, expectedSite.Display, expectedSite.Name, expectedSite.Slug });
            updated.Status.Value.Should().Be(update.Status);
            updated.Description.Should().Be(update.Description);
        }

        DeviceDetailDto fetched = RequireDevice(await DevicesClient.GetDeviceAsync(created.Id), HttpStatusCode.OK);
        fetched.Should().BeEquivalentTo(updated);
        AssertPersistedDevice(fetched);
    }

    [Test]
    [AllureStory("Deleting a Device removes it from the REST API and PostgreSQL")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    public async Task Verify_DeviceRemovedAcrossLayers_When_DeviceIsDeleted()
    {
        // Arrange
        DevicePrerequisites prerequisites = await DeviceSteps.CreatePrerequisitesAsync();
        CreateDeviceDto request = BuildDevice(prerequisites);
        DeviceDetailDto created = RequireDevice(await DeviceSteps.CreateDeviceAsync(request), HttpStatusCode.Created);

        // Act
        RestResponse response = await DevicesClient.DeleteDeviceAsync(created.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        RestResponse<DeviceDetailDto> fetched = await DevicesClient.GetDeviceAsync(created.Id);
        using (new AssertionScope())
        {
            fetched.StatusCode.Should().Be(HttpStatusCode.NotFound);
            DevicesDatabaseRepository.GetByName(request.Name).Should().BeNull();
        }
    }

    [Test]
    [AllureStory("Filtering by site includes all owned matching Devices and excludes a Device at another site")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public async Task Verify_OnlyMatchingSiteDevicesReturned_When_DevicesAreFilteredBySite()
    {
        // Arrange
        DevicePrerequisites prerequisites = await DeviceSteps.CreatePrerequisitesAsync();
        SiteDetailDto otherSite = await DeviceSteps.CreateSiteAsync();
        DeviceDetailDto first = RequireDevice(await DeviceSteps.CreateDeviceAsync(BuildDevice(prerequisites)), HttpStatusCode.Created);
        DeviceDetailDto second = RequireDevice(await DeviceSteps.CreateDeviceAsync(BuildDevice(prerequisites)), HttpStatusCode.Created);
        CreateDeviceDto otherRequest = BuildDevice(prerequisites) with { Site = otherSite.Id };
        DeviceDetailDto other = RequireDevice(await DeviceSteps.CreateDeviceAsync(otherRequest), HttpStatusCode.Created);

        // Act
        RestResponse<PagedResultDto<DeviceDetailDto>> response = await DevicesClient.FindDevicesBySiteAsync(prerequisites.Site.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        using (new AssertionScope())
        {
            response.Data!.Count.Should().Be(2);
            response.Data.Results.Should().BeEquivalentTo(new[] { first, second });
            response.Data.Results.Should().OnlyContain(x => x.Site.Id == prerequisites.Site.Id);
            response.Data.Results.Should().NotContain(x => x.Id == other.Id);
        }

        AssertPersistedDevice(first);
        AssertPersistedDevice(second);
        AssertPersistedDevice(other);
    }

    [TestCase(false, TestName = "Verify_PrerequisitesRemoved_When_ScenarioStopsBeforeDeviceCreation")]
    [TestCase(true, TestName = "Verify_DevicesAndPrerequisitesRemoved_When_ScenarioStopsAfterDeviceCreation")]
    [AllureStory("Cleanup removes owned records in dependency order after an interrupted scenario")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    public async Task Verify_OwnedRecordsRemoved_When_ScenarioIsInterrupted(bool createDevice)
    {
        // Arrange
        DevicePrerequisites prerequisites = await DeviceSteps.CreatePrerequisitesAsync();
        CreateDeviceDto request = BuildDevice(prerequisites);
        DeviceDetailDto? created = createDevice
            ? RequireDevice(await DeviceSteps.CreateDeviceAsync(request), HttpStatusCode.Created)
            : null;

        // Act
        try
        {
            throw new InvalidOperationException("Simulated scenario interruption.");
        }
        catch (InvalidOperationException exception) when (exception.Message == "Simulated scenario interruption.")
        {
            // The simulated body failure must not prevent the fixture's cleanup stack from running.
        }
        finally
        {
            await ScenarioCleanupActions.CleanUpAsync();
        }

        // Assert
        RestResponse manufacturer = await GetClient<ManufacturersApiClient>().GetManufacturerAsync(prerequisites.Manufacturer.Id);
        RestResponse deviceType = await GetClient<DeviceTypesApiClient>().GetDeviceTypeAsync(prerequisites.DeviceType.Id);
        RestResponse role = await GetClient<DeviceRolesApiClient>().GetDeviceRoleAsync(prerequisites.Role.Id);
        RestResponse site = await GetClient<SitesApiClient>().GetSiteAsync(prerequisites.Site.Id);
        RestResponse<DeviceDetailDto>? device = created is null ? null : await DevicesClient.GetDeviceAsync(created.Id);
        using (new AssertionScope())
        {
            manufacturer.StatusCode.Should().Be(HttpStatusCode.NotFound);
            deviceType.StatusCode.Should().Be(HttpStatusCode.NotFound);
            role.StatusCode.Should().Be(HttpStatusCode.NotFound);
            site.StatusCode.Should().Be(HttpStatusCode.NotFound);
            device?.StatusCode.Should().Be(HttpStatusCode.NotFound);

            DevicesDatabaseRepository.GetByName(request.Name).Should().BeNull();
        }
    }

    private static CreateDeviceDto BuildDevice(DevicePrerequisites prerequisites) =>
        new CreateDeviceDtoBuilder().Default()
            .With(x => x.DeviceType = prerequisites.DeviceType.Id)
            .With(x => x.Role = prerequisites.Role.Id)
            .With(x => x.Site = prerequisites.Site.Id).Build();

    private static DeviceDetailDto RequireDevice(RestResponse<DeviceDetailDto> response, HttpStatusCode expectedStatus)
    {
        response.StatusCode.Should().Be(expectedStatus);
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().BePositive();
        return response.Data;
    }

    private static void AssertRequestedDevice(DeviceDetailDto actual, CreateDeviceDto request) =>
        actual.Should().BeEquivalentTo(new
        {
            request.Name,
            request.Description,
            Site = new { Id = request.Site },
            DeviceType = new { Id = request.DeviceType },
            Role = new { Id = request.Role },
            Status = new { Value = request.Status }
        }, options => options.ExcludingMissingMembers(),
        "Id, Url, Display and nested display labels are generated by NetBox");

    private static void AssertPersistedDevice(DeviceDetailDto device)
    {
        DeviceRowDto? row = DevicesDatabaseRepository.GetByName(device.Name!);
        row.Should().NotBeNull();
        row.Should().BeEquivalentTo(new DeviceRowDto
        {
            Id = device.Id,
            Name = device.Name,
            SiteId = device.Site.Id,
            DeviceTypeId = device.DeviceType.Id,
            RoleId = device.Role.Id,
            Status = device.Status.Value,
            Description = device.Description
        }, "all persisted Device fields represented by the API DTO must match; Url and display labels are derived");
    }
}
