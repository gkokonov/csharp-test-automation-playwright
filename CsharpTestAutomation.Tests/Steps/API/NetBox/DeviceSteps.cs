using System.Net;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Tests.Api.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Tests.Api.Dtos.DeviceRoles;
using CsharpTestAutomation.Tests.Api.Dtos.Devices;
using CsharpTestAutomation.Tests.Api.Dtos.DeviceTypes;
using CsharpTestAutomation.Tests.Api.Dtos.Manufacturers;
using CsharpTestAutomation.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Tests.Api.Factories;
using RestSharp;

namespace CsharpTestAutomation.Tests.Steps.Api.NetBox;

/// <summary>
/// Creates test-owned Device prerequisites and registers their API cleanup on the fixture's stack.
/// Clients are borrowed from the fixture and stay alive until its cleanup finishes.
/// </summary>
public sealed class DeviceSteps(
    ManufacturersApiClient manufacturers,
    DeviceTypesApiClient deviceTypes,
    DeviceRolesApiClient roles,
    SitesApiClient sites,
    DevicesApiClient devices,
    ScenarioCleanupActions cleanup)
{
    public async Task<DevicePrerequisites> CreatePrerequisitesAsync()
    {
        CreateManufacturerDto manufacturerRequest = new CreateManufacturerDtoBuilder().Default().Build();
        RestResponse<ManufacturerDto> manufacturerResponse = await manufacturers.CreateManufacturerAsync(manufacturerRequest);
        if (manufacturerResponse.Data is { Id: > 0 } manufacturerData)
        {
            cleanup.AddCleanUpAction(() => DeleteAsync(() => manufacturers.DeleteManufacturerAsync(manufacturerData.Id), "manufacturer", manufacturerData.Id));
        }

        ManufacturerDto manufacturer = RequireCreated(manufacturerResponse, "manufacturer");
        CreateDeviceTypeDto deviceTypeRequest = new CreateDeviceTypeDtoBuilder().Default()
            .With(x => x.Manufacturer = manufacturer.Id).Build();
        RestResponse<DeviceTypeDto> deviceTypeResponse = await deviceTypes.CreateDeviceTypeAsync(deviceTypeRequest);
        if (deviceTypeResponse.Data is { Id: > 0 } deviceTypeData)
        {
            cleanup.AddCleanUpAction(() => DeleteAsync(() => deviceTypes.DeleteDeviceTypeAsync(deviceTypeData.Id), "device type", deviceTypeData.Id));
        }

        DeviceTypeDto deviceType = RequireCreated(deviceTypeResponse, "device type");
        CreateDeviceRoleDto roleRequest = new CreateDeviceRoleDtoBuilder().Default().Build();
        RestResponse<DeviceRoleDto> roleResponse = await roles.CreateDeviceRoleAsync(roleRequest);
        if (roleResponse.Data is { Id: > 0 } roleData)
        {
            cleanup.AddCleanUpAction(() => DeleteAsync(() => roles.DeleteDeviceRoleAsync(roleData.Id), "device role", roleData.Id));
        }

        DeviceRoleDto role = RequireCreated(roleResponse, "device role");
        SiteDetailDto site = await CreateSiteAsync();
        return new DevicePrerequisites(manufacturer, deviceType, role, site);
    }

    /// <summary>Create additional sites before their devices so LIFO cleanup can delete dependents first.</summary>
    public async Task<SiteDetailDto> CreateSiteAsync()
    {
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> response = await sites.CreateSiteAsync(request);
        if (response.Data is { Id: > 0 } site)
        {
            cleanup.AddCleanUpAction(() => DeleteAsync(() => sites.DeleteSiteAsync(site.Id), "site", site.Id));
        }

        return RequireCreated(response, "site");
    }

    public async Task<RestResponse<DeviceDetailDto>> CreateDeviceAsync(CreateDeviceDto request)
    {
        RestResponse<DeviceDetailDto> response = await devices.CreateDeviceAsync(request);
        if (response.Data is { Id: > 0 } device)
        {
            cleanup.AddCleanUpAction(() => DeleteAsync(() => devices.DeleteDeviceAsync(device.Id), "device", device.Id));
        }

        return response;
    }

    /// <summary>Register before UI submission so cleanup still finds the Device if navigation or assertions fail.</summary>
    public void RegisterUiDeviceCleanup(string name, int siteId) => cleanup.AddCleanUpAction(async () =>
    {
        RestResponse<PagedResultDto<DeviceDetailDto>> response = await devices.FindDevicesByNameAsync(name);
        if (response.StatusCode != HttpStatusCode.OK || response.Data is null)
        {
            throw new InvalidOperationException($"Could not find the UI-created device for cleanup: {response.StatusCode}.");
        }

        foreach (DeviceDetailDto device in response.Data.Results.Where(x => x.Name == name && x.Site.Id == siteId))
        {
            await DeleteAsync(() => devices.DeleteDeviceAsync(device.Id), "device", device.Id);
        }
    });

    private static T RequireCreated<T>(RestResponse<T> response, string resource) where T : class
    {
        if (response.StatusCode != HttpStatusCode.Created || response.Data is null)
        {
            throw new InvalidOperationException($"Could not create Device prerequisite {resource}: {response.StatusCode} ({response.ResponseStatus}).");
        }

        return response.Data;
    }

    private static async Task DeleteAsync(Func<Task<RestResponse>> delete, string resource, int id)
    {
        RestResponse response = await delete();
        if (response.StatusCode is not (HttpStatusCode.NoContent or HttpStatusCode.NotFound))
        {
            throw new InvalidOperationException($"Failed to clean up {resource} {id}: {response.StatusCode} ({response.ResponseStatus}).");
        }
    }
}
