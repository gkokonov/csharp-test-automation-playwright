using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.DeviceRoles;
using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Tests.Api.Clients;

/// <summary>
/// Minimal client for prerequisite setup only; no full CRUD test coverage is required for this resource.
/// </summary>
public sealed class DeviceRolesApiClient(IRestClientFactory factory, IAuthenticator? authenticator = null) : IDisposable
{
    private const string ServiceName = "netbox";
    private const string Resource = "dcim/device-roles/";

    private readonly IRestClient _client = factory.Create(ServiceName, authenticator);

    public Task<RestResponse<DeviceRoleDto>> CreateDeviceRoleAsync(CreateDeviceRoleDto role, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(Resource, Method.Post).AddJsonBody(role);
        return _client.ExecuteAsync<DeviceRoleDto>(request, cancellationToken);
    }

    public Task<RestResponse<DeviceRoleDto>> GetDeviceRoleAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{Resource}{id}/");
        return _client.ExecuteAsync<DeviceRoleDto>(request, cancellationToken);
    }

    public Task<RestResponse> DeleteDeviceRoleAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{Resource}{id}/", Method.Delete);
        return _client.ExecuteAsync(request, cancellationToken);
    }

    public void Dispose() => (_client as IDisposable)?.Dispose();
}
