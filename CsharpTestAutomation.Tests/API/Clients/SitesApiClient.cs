using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Tests.Api.Dtos.Sites;
using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Tests.Api.Clients;

public sealed class SitesApiClient(IRestClientFactory factory, IAuthenticator? authenticator = null) : IDisposable
{
    private const string ServiceName = "netbox";
    private const string Resource = "dcim/sites/";

    private readonly IRestClient _client = factory.Create(ServiceName, authenticator);

    public Task<RestResponse<SiteDetailDto>> CreateSiteAsync(CreateSiteDto site, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(Resource, Method.Post).AddJsonBody(site);
        return _client.ExecuteAsync<SiteDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse<SiteDetailDto>> GetSiteAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{Resource}{id}/");
        return _client.ExecuteAsync<SiteDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse<PagedResultDto<SiteDetailDto>>> FindSitesBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest(Resource).AddQueryParameter("slug", slug);
        return _client.ExecuteAsync<PagedResultDto<SiteDetailDto>>(request, cancellationToken);
    }

    public Task<RestResponse<SiteDetailDto>> UpdateSiteAsync(int id, UpdateSiteDto update, CancellationToken cancellationToken = default)
    {
        RestRequest request = new RestRequest($"{Resource}{id}/", Method.Patch).AddJsonBody(update);
        return _client.ExecuteAsync<SiteDetailDto>(request, cancellationToken);
    }

    public Task<RestResponse> DeleteSiteAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"{Resource}{id}/", Method.Delete);
        return _client.ExecuteAsync(request, cancellationToken);
    }

    public void Dispose() => (_client as IDisposable)?.Dispose();
}
