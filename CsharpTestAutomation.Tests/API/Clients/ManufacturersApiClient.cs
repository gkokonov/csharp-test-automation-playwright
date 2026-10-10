using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.Manufacturers;
using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Tests.Api.Clients;

/// <summary>
/// Minimal client for prerequisite setup only; no full CRUD test coverage is required for this resource.
/// </summary>
public sealed class ManufacturersApiClient(IRestClientFactory factory, IAuthenticator? authenticator = null) : IDisposable
{
    private const string ServiceName = "netbox";
    private const string Resource = "dcim/manufacturers/";

    private readonly IRestClient _client = factory.Create(ServiceName, authenticator);

    public Task<RestResponse<ManufacturerResponseDto>> CreateManufacturerAsync(CreateManufacturerDto manufacturer, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(Resource, Method.Post).AddJsonBody(manufacturer);
        return _client.ExecuteAsync<ManufacturerResponseDto>(request, cancellationToken);
    }

    public Task<RestResponse<ManufacturerResponseDto>> GetManufacturerAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{Resource}{id}/");
        return _client.ExecuteAsync<ManufacturerResponseDto>(request, cancellationToken);
    }

    public Task<RestResponse> DeleteManufacturerAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{Resource}{id}/", Method.Delete);
        return _client.ExecuteAsync(request, cancellationToken);
    }

    public void Dispose() => (_client as IDisposable)?.Dispose();
}
