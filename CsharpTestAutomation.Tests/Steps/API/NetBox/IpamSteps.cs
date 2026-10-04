using System.Net;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Tests.Api.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Tests.Api.Dtos.Ipam;
using RestSharp;

namespace CsharpTestAutomation.Tests.Steps.Api.NetBox;

/// <summary>Registers owned IPAM records on the fixture's LIFO cleanup stack; clients are borrowed.</summary>
public sealed class IpamSteps(IpamApiClient client, ScenarioCleanupActions cleanup)
{
    public async Task<RestResponse<PrefixDto>> CreatePrefixAsync(CreatePrefixDto request)
    {
        RestResponse<PrefixDto> response = await client.CreatePrefixAsync(request);
        if (response.Data is { Id: > 0 } prefix)
        {
            RegisterPrefixCleanup(prefix.Id);
        }

        return response;
    }

    /// <summary>Create the prerequisite Prefix first so cleanup deletes its addresses before it.</summary>
    public async Task<RestResponse<IpAddressDto>> CreateIpAddressAsync(CreateIpAddressDto request)
    {
        RestResponse<IpAddressDto> response = await client.CreateIpAddressAsync(request);
        if (response.Data is { Id: > 0 } address)
        {
            cleanup.AddCleanUpAction(() => DeleteAsync(() => client.DeleteIpAddressAsync(address.Id), "IP address", address.Id));
        }

        return response;
    }

    public void RegisterPrefixCleanup(int id) =>
        cleanup.AddCleanUpAction(() => DeleteAsync(() => client.DeletePrefixAsync(id), "prefix", id));

    /// <summary>Register before form submission; a unique description identifies the owned Prefix if navigation fails.</summary>
    public void RegisterUiPrefixCleanup(CreatePrefixDto request) => cleanup.AddCleanUpAction(async () =>
    {
        RestResponse<PagedResultDto<PrefixDto>> response = await client.FindPrefixesByCidrAsync(request.Prefix, request.Description);
        if (response.StatusCode != HttpStatusCode.OK || response.Data is null)
        {
            throw new InvalidOperationException($"Could not find the UI-created prefix for cleanup: {response.StatusCode}.");
        }

        foreach (PrefixDto prefix in response.Data.Results.Where(x => x.Prefix == request.Prefix && x.Description == request.Description))
        {
            await DeleteAsync(() => client.DeletePrefixAsync(prefix.Id), "prefix", prefix.Id);
        }
    });

    private static async Task DeleteAsync(Func<Task<RestResponse>> delete, string resource, int id)
    {
        RestResponse response = await delete();
        if (response.StatusCode is not (HttpStatusCode.NoContent or HttpStatusCode.NotFound))
        {
            throw new InvalidOperationException($"Failed to clean up {resource} {id}: {response.StatusCode} ({response.ResponseStatus}).");
        }
    }
}
