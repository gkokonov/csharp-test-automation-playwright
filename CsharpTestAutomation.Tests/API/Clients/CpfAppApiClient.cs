using System.Net;
using AwesomeAssertions;
using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Tests.Api.Dtos;
using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Tests.Api.Clients;

public sealed class CpfAppApiClient(IRestClientFactory factory, IAuthenticator? authenticator = null) : IDisposable
{
    private const string ServiceName = "cpfappqa";

    private readonly IRestClient _client = factory.Create(ServiceName, authenticator);

    public Task<RestResponse<List<CpfListItemDto>>> GetCpfsAsync(IAuthenticator? requestAuthenticator = null, CancellationToken ct = default)
    {
        RestRequest request = new("/api/cpfs", Method.Get);
        if (requestAuthenticator != null)
            request.Authenticator = requestAuthenticator;
        return _client.ExecuteAsync<List<CpfListItemDto>>(request, ct);
    }

    public Task<RestResponse<List<CpfListItemDto>>> GetCpfsAsync(string searchTerm, IAuthenticator? requestAuthenticator = null, CancellationToken ct = default)
    {
        RestRequest request = new("/api/cpfs", Method.Get);
        request.AddQueryParameter("searchTerm", searchTerm);
        if (requestAuthenticator != null)
            request.Authenticator = requestAuthenticator;
        return _client.ExecuteAsync<List<CpfListItemDto>>(request, ct);
    }

    public Task<RestResponse<CpfListItemDto>> GetCpfByIdAsync(Guid id, IAuthenticator? requestAuthenticator = null, CancellationToken ct = default)
    {
        RestRequest request = new($"/api/cpfs/{id}", Method.Get);
        if (requestAuthenticator != null)
            request.Authenticator = requestAuthenticator;
        return _client.ExecuteAsync<CpfListItemDto>(request, ct);
    }

    public Task<RestResponse<CpfListItemDto>> GetCpfByRawIdAsync(string id, CancellationToken ct = default)
        => _client.ExecuteAsync<CpfListItemDto>(new RestRequest($"/api/cpfs/{id}", Method.Get), ct);

    public Task<RestResponse<Guid>> CreateCpfAsync(CreateCpfDto body, IAuthenticator? requestAuthenticator = null, CancellationToken ct = default)
    {
        RestRequest request = new("/api/cpfs", Method.Post);
        request.AddJsonBody(body);
        if (requestAuthenticator != null)
            request.Authenticator = requestAuthenticator;
        return _client.ExecuteAsync<Guid>(request, ct);
    }

    public Task<RestResponse> DeleteCpfAsync(Guid id, IAuthenticator? requestAuthenticator = null, CancellationToken ct = default)
    {
        RestRequest request = new($"/api/cpfs/{id}", Method.Delete);
        if (requestAuthenticator != null)
            request.Authenticator = requestAuthenticator;
        return _client.ExecuteAsync(request, ct);
    }

    public async Task<RestResponse> DeleteCpfAndExpectNoContentAsync(Guid id, IAuthenticator? requestAuthenticator = null, CancellationToken ct = default)
    {
        RestResponse response = await DeleteCpfAsync(id, requestAuthenticator, ct);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        return response;
    }

    public void Dispose() => (_client as IDisposable)?.Dispose();
}
