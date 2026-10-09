using System.Net;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CsharpTestAutomation.Bdd.Tests.Api.Clients;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.DeviceRoles;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Devices;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.DeviceTypes;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Manufacturers;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Bdd.Tests.Api.Factories;
using CsharpTestAutomation.Bdd.Tests.Context;
using CsharpTestAutomation.Bdd.Tests.Database.NetBox.DTO;
using CsharpTestAutomation.Bdd.Tests.Database.NetBox.Queries;
using CsharpTestAutomation.Framework.Common;
using Reqnroll;
using RestSharp;

namespace CsharpTestAutomation.Bdd.Tests.Steps.Api.NetBox;

[Binding]
[Scope(Tag = "Device")]
public sealed class DeviceApiSteps(ManufacturersApiClient manufacturers, DeviceTypesApiClient deviceTypes,
    DeviceRolesApiClient roles, SitesApiClient sites, DevicesApiClient devices, ScenarioCleanupActions cleanup,
    DeviceScenarioState state, DevicesDatabaseRepository database)
{
    [Given("owned Device prerequisites and unique valid Device details")]
    public async Task PrepareDeviceAsync()
    {
        CreateManufacturerDto manufacturerRequest = new CreateManufacturerDtoBuilder().Default().Build();
        RestResponse<ManufacturerDto> manufacturerResponse = await manufacturers.CreateManufacturerAsync(manufacturerRequest);
        if (manufacturerResponse.Data is { Id: > 0 } manufacturerData)
        {
            cleanup.AddCleanUpAction(() => DeleteOwnedAsync(() => manufacturers.DeleteManufacturerAsync(manufacturerData.Id), "manufacturer", manufacturerData.Id));
        }

        ManufacturerDto manufacturer = RequireCreated(manufacturerResponse, "manufacturer");
        CreateDeviceTypeDto typeRequest = new CreateDeviceTypeDtoBuilder().Default()
            .With(x => x.Manufacturer = manufacturer.Id).Build();
        RestResponse<DeviceTypeDto> typeResponse = await deviceTypes.CreateDeviceTypeAsync(typeRequest);
        if (typeResponse.Data is { Id: > 0 } typeData)
        {
            cleanup.AddCleanUpAction(() => DeleteOwnedAsync(() => deviceTypes.DeleteDeviceTypeAsync(typeData.Id), "device type", typeData.Id));
        }

        DeviceTypeDto deviceType = RequireCreated(typeResponse, "device type");
        CreateDeviceRoleDto roleRequest = new CreateDeviceRoleDtoBuilder().Default().Build();
        RestResponse<DeviceRoleDto> roleResponse = await roles.CreateDeviceRoleAsync(roleRequest);
        if (roleResponse.Data is { Id: > 0 } roleData)
        {
            cleanup.AddCleanUpAction(() => DeleteOwnedAsync(() => roles.DeleteDeviceRoleAsync(roleData.Id), "device role", roleData.Id));
        }

        DeviceRoleDto role = RequireCreated(roleResponse, "device role");
        SiteDetailDto site = await CreateOwnedSiteAsync();
        state.Prerequisites = new(manufacturer, deviceType, role, site);
        state.ExpectedSite = site;
        state.Request = BuildDevice();
    }

    [Given("an owned active Device")]
    public async Task PrepareOwnedDeviceAsync()
    {
        await PrepareDeviceAsync();
        await CreatePrerequisiteDeviceAsync();
    }

    [Given("an owned active Device at the original Site")]
    public async Task CreatePrerequisiteDeviceAsync() =>
        state.Created = RequireDevice(await CreateOwnedDeviceAsync(state.Request), HttpStatusCode.Created);

    [Given("another owned Device Site")]
    public async Task PrepareOtherSiteAsync() => state.OtherSite = await CreateOwnedSiteAsync();

    [Given("valid Device status and description changes")]
    public void PrepareUpdate() => state.Update = new UpdateDeviceDtoBuilder().Default().Build();

    [Given("valid Device changes that move it to the other Site")]
    public void PrepareMove()
    {
        state.ExpectedSite = state.OtherSite!;
        state.Update = new UpdateDeviceDtoBuilder().Default().With(x => x.Site = state.ExpectedSite.Id).Build();
    }

    [Given("two owned Devices at one Site and another Device at a different Site")]
    public async Task PrepareFilterAsync()
    {
        await PrepareDeviceAsync();
        state.OtherSite = await CreateOwnedSiteAsync();
        state.OwnedDevices.Add(RequireDevice(await CreateOwnedDeviceAsync(BuildDevice()), HttpStatusCode.Created));
        state.OwnedDevices.Add(RequireDevice(await CreateOwnedDeviceAsync(BuildDevice()), HttpStatusCode.Created));
        CreateDeviceDto otherRequest = BuildDevice() with { Site = state.OtherSite.Id };
        state.OwnedDevices.Add(RequireDevice(await CreateOwnedDeviceAsync(otherRequest), HttpStatusCode.Created));
    }

    [When("the Device is created through the service")]
    public async Task CreateAsync() => state.Api.DetailResponse = await CreateOwnedDeviceAsync(state.Request);

    [When("the Device is found by its exact name")]
    public async Task FindAsync() => state.Api.SearchResponse = await devices.FindDevicesByNameAsync(state.Request.Name);

    [When("the Device changes are submitted through the service")]
    public async Task UpdateAsync() => state.Api.DetailResponse = await devices.UpdateDeviceAsync(state.Created!.Id, state.Update);

    [When("the Device is deleted through the service")]
    public async Task DeleteAsync() => state.Api.DeleteResponse = await devices.DeleteDeviceAsync(state.Created!.Id);

    [When("Devices are found by the original Site")]
    public async Task FilterAsync() => state.Api.SearchResponse = await devices.FindDevicesBySiteAsync(state.Prerequisites.Site.Id);

    [When("the Device scenario is interrupted and cleanup runs")]
    public async Task InterruptAsync()
    {
        try
        {
            throw new InvalidOperationException("Simulated scenario interruption.");
        }
        catch (InvalidOperationException exception) when (exception.Message == "Simulated scenario interruption.")
        {
            // Simulate a body failure while allowing the cleanup regression to report its assertions.
        }
        finally
        {
            await cleanup.CleanUpAsync();
        }
    }

    [Then("the requested Device details are returned")]
    public void VerifyCreation()
    {
        DeviceDetailDto created = RequireDevice(state.Api.DetailResponse, HttpStatusCode.Created);
        created.Should().BeEquivalentTo(new
        {
            state.Request.Name,
            state.Request.Description,
            Site = new { Id = state.Request.Site },
            DeviceType = new { Id = state.Request.DeviceType },
            Role = new { Id = state.Request.Role },
            Status = new { Value = state.Request.Status }
        }, options => options.ExcludingMissingMembers(),
            "Id, Url, Display and nested display labels are generated by NetBox");
        state.PersistedDevice = created;
    }

    [Then("only the expected Device is returned")]
    public void VerifySearch()
    {
        state.Api.SearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        state.Api.SearchResponse.Data.Should().NotBeNull();
        state.Api.SearchResponse.Data!.Count.Should().Be(1);
        state.Api.SearchResponse.Data.Results.Should().ContainSingle().Which.Should().BeEquivalentTo(state.Created);
        state.PersistedDevice = state.Created!;
    }

    [Then("the service saves the Device changes and retains its other details")]
    public async Task VerifyUpdateAsync()
    {
        DeviceDetailDto updated = RequireDevice(state.Api.DetailResponse, HttpStatusCode.OK);
        state.Created.Should().NotBeNull();
        DeviceDetailDto created = state.Created!;
        SiteDetailDto expectedSite = state.ExpectedSite;
        using (new AssertionScope())
        {
            updated.Should().BeEquivalentTo(created, options => options
                .Excluding(x => x.Site).Excluding(x => x.Status).Excluding(x => x.Description),
                "only Site, Status and Description were requested to change");
            updated.Site.Should().BeEquivalentTo(new { expectedSite.Id, expectedSite.Url, expectedSite.Display, expectedSite.Name, expectedSite.Slug });
            updated.Status.Value.Should().Be(state.Update.Status);
            updated.Description.Should().Be(state.Update.Description);
        }

        DeviceDetailDto fetched = RequireDevice(await devices.GetDeviceAsync(state.Created!.Id), HttpStatusCode.OK);
        fetched.Should().BeEquivalentTo(updated);
        state.PersistedDevice = fetched;
    }

    [Then("the Device is unavailable through the service")]
    public async Task VerifyDeletionAsync()
    {
        state.Api.DeleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        RestResponse<DeviceDetailDto> response = await devices.GetDeviceAsync(state.Created!.Id);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Then("only the Devices at the original Site are returned")]
    public void VerifyFilter()
    {
        state.Api.SearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        state.Api.SearchResponse.Data.Should().NotBeNull();
        using (new AssertionScope())
        {
            state.Api.SearchResponse.Data!.Count.Should().Be(2);
            state.Api.SearchResponse.Data.Results.Should().BeEquivalentTo(state.OwnedDevices.Take(2));
            state.Api.SearchResponse.Data.Results.Should().OnlyContain(x => x.Site.Id == state.Prerequisites.Site.Id);
            state.Api.SearchResponse.Data.Results.Should().NotContain(x => x.Id == state.OwnedDevices[2].Id);
        }
    }

    [Then("all owned Device records and prerequisites are unavailable")]
    public async Task VerifyCleanupAsync()
    {
        DevicePrerequisites prerequisites = state.Prerequisites;
        RestResponse manufacturer = await manufacturers.GetManufacturerAsync(prerequisites.Manufacturer.Id);
        RestResponse deviceType = await deviceTypes.GetDeviceTypeAsync(prerequisites.DeviceType.Id);
        RestResponse role = await roles.GetDeviceRoleAsync(prerequisites.Role.Id);
        RestResponse site = await sites.GetSiteAsync(prerequisites.Site.Id);
        RestResponse<DeviceDetailDto>? device = state.Created is null ? null : await devices.GetDeviceAsync(state.Created.Id);
        using (new AssertionScope())
        {
            manufacturer.StatusCode.Should().Be(HttpStatusCode.NotFound);
            deviceType.StatusCode.Should().Be(HttpStatusCode.NotFound);
            role.StatusCode.Should().Be(HttpStatusCode.NotFound);
            site.StatusCode.Should().Be(HttpStatusCode.NotFound);
            if (state.Created is not null)
            {
                device.Should().NotBeNull();
                device!.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }

    [Then("the Device details are saved")]
    public Task VerifyPersistenceAsync() => AssertPersistedDeviceAsync(state.PersistedDevice);

    [Then("all owned Device details are saved")]
    public async Task VerifyAllPersistenceAsync()
    {
        foreach (DeviceDetailDto device in state.OwnedDevices)
        {
            await AssertPersistedDeviceAsync(device);
        }
    }

    [Then("the Device is no longer saved")]
    public async Task VerifyAbsenceAsync() => (await database.GetByNameAsync(state.Request.Name)).Should().BeNull();

    internal static async Task DeleteOwnedAsync(Func<Task<RestResponse>> delete, string resource, int id)
    {
        RestResponse response = await delete();
        if (response.StatusCode is not (HttpStatusCode.NoContent or HttpStatusCode.NotFound))
        {
            throw new InvalidOperationException($"Failed to clean up {resource} {id}: {response.StatusCode} ({response.ResponseStatus}).");
        }
    }

    private CreateDeviceDto BuildDevice() => new CreateDeviceDtoBuilder().Default()
        .With(x => x.DeviceType = state.Prerequisites.DeviceType.Id)
        .With(x => x.Role = state.Prerequisites.Role.Id)
        .With(x => x.Site = state.Prerequisites.Site.Id).Build();

    private async Task<SiteDetailDto> CreateOwnedSiteAsync()
    {
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> response = await sites.CreateSiteAsync(request);
        if (response.Data is { Id: > 0 } site)
        {
            cleanup.AddCleanUpAction(() => DeleteOwnedAsync(() => sites.DeleteSiteAsync(site.Id), "site", site.Id));
        }

        return RequireCreated(response, "site");
    }

    private async Task<RestResponse<DeviceDetailDto>> CreateOwnedDeviceAsync(CreateDeviceDto request)
    {
        RestResponse<DeviceDetailDto> response = await devices.CreateDeviceAsync(request);
        if (response.Data is { Id: > 0 } device)
        {
            cleanup.AddCleanUpAction(() => DeleteOwnedAsync(() => devices.DeleteDeviceAsync(device.Id), "device", device.Id));
        }

        return response;
    }

    private static T RequireCreated<T>(RestResponse<T> response, string resource) where T : class
    {
        if (response.StatusCode != HttpStatusCode.Created || response.Data is null)
        {
            throw new InvalidOperationException($"Could not create Device prerequisite {resource}: {response.StatusCode} ({response.ResponseStatus}).");
        }

        return response.Data;
    }

    private static DeviceDetailDto RequireDevice(RestResponse<DeviceDetailDto> response, HttpStatusCode expectedStatus)
    {
        response.StatusCode.Should().Be(expectedStatus);
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().BePositive();
        return response.Data;
    }

    private async Task AssertPersistedDeviceAsync(DeviceDetailDto device)
    {
        DeviceRowDto? row = await database.GetByNameAsync(device.Name!);
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
