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

    public Task<RestResponse<PrefixDetailDto>> CreatePrefixAsync(CreatePrefixDto prefix, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(PrefixesResource, Method.Post).AddJsonBody(prefix);
        return _client.ExecuteAsync<PrefixDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse<PrefixDetailDto>> GetPrefixAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{PrefixesResource}{id}/");
        return _client.ExecuteAsync<PrefixDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse<PagedResultDto<PrefixDetailDto>>> FindPrefixesByCidrAsync(
        string prefix, string? description = null, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(PrefixesResource).AddQueryParameter("prefix", prefix);
        if (description is not null)
        {
            request.AddQueryParameter("description", description);
        }

        return _client.ExecuteAsync<PagedResultDto<PrefixDetailDto>>(request, cancellationToken);
    }

    public Task<RestResponse<PrefixDetailDto>> UpdatePrefixAsync(int id, UpdatePrefixDto update, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest($"{PrefixesResource}{id}/", Method.Patch).AddJsonBody(update);
        return _client.ExecuteAsync<PrefixDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse> DeletePrefixAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{PrefixesResource}{id}/", Method.Delete);
        return _client.ExecuteAsync(request, cancellationToken);
    }

    public Task<RestResponse<IpAddressDetailDto>> CreateIpAddressAsync(CreateIpAddressDto ipAddress, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(IpAddressesResource, Method.Post).AddJsonBody(ipAddress);
        return _client.ExecuteAsync<IpAddressDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse<IpAddressDetailDto>> GetIpAddressAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{IpAddressesResource}{id}/");
        return _client.ExecuteAsync<IpAddressDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse<PagedResultDto<IpAddressDetailDto>>> FindIpAddressesByAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(IpAddressesResource).AddQueryParameter("address", address);
        return _client.ExecuteAsync<PagedResultDto<IpAddressDetailDto>>(request, cancellationToken);
    }

    public Task<RestResponse<IpAddressDetailDto>> UpdateIpAddressAsync(int id, UpdateIpAddressDto update, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest($"{IpAddressesResource}{id}/", Method.Patch).AddJsonBody(update);
        return _client.ExecuteAsync<IpAddressDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse> DeleteIpAddressAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{IpAddressesResource}{id}/", Method.Delete);
        return _client.ExecuteAsync(request, cancellationToken);
    }

    public void Dispose() => (_client as IDisposable)?.Dispose();
}
