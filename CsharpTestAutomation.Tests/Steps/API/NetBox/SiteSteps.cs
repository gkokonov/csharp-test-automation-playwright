using System.Net;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Tests.Api.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Tests.Api.Dtos.Sites;
using RestSharp;

namespace CsharpTestAutomation.Tests.Steps.Api.NetBox;

/// <summary>Registers owned Sites on the fixture's cleanup stack; the client is borrowed.</summary>
public sealed class SiteSteps(SitesApiClient client, ScenarioCleanupActions cleanup)
{
    public async Task<RestResponse<SiteDetailDto>> CreateSiteAsync(CreateSiteDto request)
    {
        RestResponse<SiteDetailDto> response = await client.CreateSiteAsync(request);
        if (response.Data is { Id: > 0 } site)
        {
            RegisterSiteCleanup(site.Id);
        }

        return response;
    }

    public void RegisterSiteCleanup(int id) => cleanup.AddCleanUpAction(() => DeleteSiteAsync(id));

    /// <summary>Register before UI submission so later navigation or assertion failures cannot skip cleanup.</summary>
    public void RegisterUiSiteCleanup(string slug) => cleanup.AddCleanUpAction(async () =>
    {
        RestResponse<PagedResultDto<SiteDetailDto>> response = await client.FindSitesBySlugAsync(slug);
        if (response.StatusCode != HttpStatusCode.OK || response.Data is null)
        {
            throw new InvalidOperationException($"Could not find the UI-created site for cleanup: {response.StatusCode} ({response.ResponseStatus}).");
        }

        foreach (SiteDetailDto site in response.Data.Results.Where(x => x.Slug == slug))
        {
            await DeleteSiteAsync(site.Id);
        }
    });

    private async Task DeleteSiteAsync(int id)
    {
        RestResponse response = await client.DeleteSiteAsync(id);
        if (response.StatusCode is not (HttpStatusCode.NoContent or HttpStatusCode.NotFound))
        {
            throw new InvalidOperationException($"Failed to clean up site {id}: {response.StatusCode} ({response.ResponseStatus}).");
        }
    }
}
