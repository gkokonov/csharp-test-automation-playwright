using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Tests.Api.Dtos.Devices;
using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Tests.Api.Clients;

public sealed class DevicesApiClient(IRestClientFactory factory, IAuthenticator? authenticator = null) : IDisposable
{
    private const string ServiceName = "netbox";
    private const string Resource = "dcim/devices/";

    private readonly IRestClient _client = factory.Create(ServiceName, authenticator);

    public Task<RestResponse<DeviceDetailDto>> CreateDeviceAsync(CreateDeviceDto device, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(Resource, Method.Post).AddJsonBody(device);
        return _client.ExecuteAsync<DeviceDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse<DeviceDetailDto>> GetDeviceAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{Resource}{id}/");
        return _client.ExecuteAsync<DeviceDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse<PagedResultDto<DeviceDetailDto>>> FindDevicesByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(Resource).AddQueryParameter("name", name);
        return _client.ExecuteAsync<PagedResultDto<DeviceDetailDto>>(request, cancellationToken);
    }

    public Task<RestResponse<DeviceDetailDto>> UpdateDeviceAsync(int id, UpdateDeviceDto update, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest($"{Resource}{id}/", Method.Patch).AddJsonBody(update);
        return _client.ExecuteAsync<DeviceDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse> DeleteDeviceAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{Resource}{id}/", Method.Delete);
        return _client.ExecuteAsync(request, cancellationToken);
    }

    public void Dispose() => (_client as IDisposable)?.Dispose();
}
