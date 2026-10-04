using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Tests.Api.Dtos.Ipam;
using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Tests.Api.Clients;

public sealed class IpamApiClient(IRestClientFactory factory, IAuthenticator? authenticator = null) : IDisposable
{
    private const string ServiceName = "netbox";
    private const string PrefixesResource = "ipam/prefixes/";
    private const string IpAddressesResource = "ipam/ip-addresses/";

    private readonly IRestClient _client = factory.Create(ServiceName, authenticator);

    public Task<RestResponse<PrefixDto>> CreatePrefixAsync(CreatePrefixDto prefix, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(PrefixesResource, Method.Post).AddJsonBody(prefix);
        return _client.ExecuteAsync<PrefixDto>(request, cancellationToken);
    }

    public Task<RestResponse<PrefixDto>> GetPrefixAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{PrefixesResource}{id}/");
        return _client.ExecuteAsync<PrefixDto>(request, cancellationToken);
    }

    public Task<RestResponse<PagedResultDto<PrefixDto>>> FindPrefixesByCidrAsync(
        string prefix, string? description = null, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(PrefixesResource).AddQueryParameter("prefix", prefix);
        if (description is not null)
        {
            request.AddQueryParameter("description", description);
        }

        return _client.ExecuteAsync<PagedResultDto<PrefixDto>>(request, cancellationToken);
    }

    public Task<RestResponse<PrefixDto>> UpdatePrefixAsync(int id, UpdatePrefixDto update, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest($"{PrefixesResource}{id}/", Method.Patch).AddJsonBody(update);
        return _client.ExecuteAsync<PrefixDto>(request, cancellationToken);
    }

    public Task<RestResponse> DeletePrefixAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{PrefixesResource}{id}/", Method.Delete);
        return _client.ExecuteAsync(request, cancellationToken);
    }

    public Task<RestResponse<IpAddressDto>> CreateIpAddressAsync(CreateIpAddressDto ipAddress, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(IpAddressesResource, Method.Post).AddJsonBody(ipAddress);
        return _client.ExecuteAsync<IpAddressDto>(request, cancellationToken);
    }

    public Task<RestResponse<IpAddressDto>> GetIpAddressAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{IpAddressesResource}{id}/");
        return _client.ExecuteAsync<IpAddressDto>(request, cancellationToken);
    }

    public Task<RestResponse<PagedResultDto<IpAddressDto>>> FindIpAddressesByAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(IpAddressesResource).AddQueryParameter("address", address);
        return _client.ExecuteAsync<PagedResultDto<IpAddressDto>>(request, cancellationToken);
    }

    public Task<RestResponse<IpAddressDto>> UpdateIpAddressAsync(int id, UpdateIpAddressDto update, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest($"{IpAddressesResource}{id}/", Method.Patch).AddJsonBody(update);
        return _client.ExecuteAsync<IpAddressDto>(request, cancellationToken);
    }

    public Task<RestResponse> DeleteIpAddressAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{IpAddressesResource}{id}/", Method.Delete);
        return _client.ExecuteAsync(request, cancellationToken);
    }

    public void Dispose() => (_client as IDisposable)?.Dispose();
}
