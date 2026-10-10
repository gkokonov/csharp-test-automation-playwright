using System.Net;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CsharpTestAutomation.Bdd.Tests.Api.Clients;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Bdd.Tests.Api.Factories;
using CsharpTestAutomation.Bdd.Tests.Context;
using CsharpTestAutomation.Bdd.Tests.Database.NetBox.DTO;
using CsharpTestAutomation.Bdd.Tests.Database.NetBox.Queries;
using CsharpTestAutomation.Framework.Common;
using Reqnroll;
using RestSharp;

namespace CsharpTestAutomation.Bdd.Tests.Steps.Api.NetBox;

[Binding]
[Scope(Tag = "Site")]
public sealed class SiteApiSteps(SitesApiClient client, ScenarioCleanupActions cleanup,
    SiteScenarioState state, SitesDatabaseRepository database)
{
    [Given("unique valid Site details")]
    public void PrepareUniqueSite() => state.CreateRequest = new CreateSiteDtoBuilder().Default().Build();

    [Given("an owned active Site")]
    public async Task CreatePrerequisiteAsync()
    {
        PrepareUniqueSite();
        var response = await CreateOwnedSiteAsync(state.CreateRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Created, "the scenario requires an owned Site");
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().BePositive();
        state.CreatedSite = response.Data;
    }

    [Given("valid changes to its status and description")]
    public void PrepareUpdate() => state.UpdateRequest = new UpdateSiteDtoBuilder().Default().Build();

    [When("the Site is created through the service")]
    public async Task CreateAsync() => state.ApiResponses.SingleResponse = await CreateOwnedSiteAsync(state.CreateRequest);

    [When("the Site is retrieved through the service")]
    public async Task RetrieveAsync() => state.ApiResponses.SingleResponse = await client.GetSiteAsync(state.CreatedSite.Id);

    [When("the Site is found by its slug")]
    public async Task FindAsync() => state.ApiResponses.SearchResponse = await client.FindSitesBySlugAsync(state.CreatedSite.Slug);

    [When("the Site changes are submitted through the service")]
    public async Task UpdateAsync() => state.ApiResponses.SingleResponse = await client.UpdateSiteAsync(state.CreatedSite.Id, state.UpdateRequest);

    [When("the Site is deleted through the service")]
    public async Task DeleteAsync() => state.ApiResponses.DeleteResponse = await client.DeleteSiteAsync(state.CreatedSite.Id);

    [Then("the requested Site details are returned")]
    public void VerifyCreatedDetails()
    {
        state.ApiResponses.SingleResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        state.ApiResponses.SingleResponse.Data.Should().NotBeNull();
        SiteDetailDto site = state.ApiResponses.SingleResponse.Data!;
        state.PersistedSite = site;
        using (new AssertionScope())
        {
            site.Id.Should().BePositive();
            site.Name.Should().Be(state.CreateRequest.Name);
            site.Slug.Should().Be(state.CreateRequest.Slug);
            site.Status.Value.Should().Be(state.CreateRequest.Status);
            site.Description.Should().Be(state.CreateRequest.Description);
        }
    }

    [Then("the original Site details are returned")]
    public void VerifyRetrievedDetails()
    {
        state.ApiResponses.SingleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        state.ApiResponses.SingleResponse.Data.Should().NotBeNull();
        state.ApiResponses.SingleResponse.Data.Should().BeEquivalentTo(state.CreatedSite);
        state.PersistedSite = state.CreatedSite;
    }

    [Then("only the expected Site is returned")]
    public void VerifySearch()
    {
        state.ApiResponses.SearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        state.ApiResponses.SearchResponse.Data.Should().NotBeNull();
        state.ApiResponses.SearchResponse.Data!.Count.Should().Be(1);
        state.ApiResponses.SearchResponse.Data.Results.Should().ContainSingle().Which.Should().BeEquivalentTo(state.CreatedSite);
    }

    [Then("the service saves the changes and retains the other Site details")]
    public async Task VerifyUpdateAsync()
    {
        state.ApiResponses.SingleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        state.ApiResponses.SingleResponse.Data.Should().NotBeNull();
        SiteDetailDto updated = state.ApiResponses.SingleResponse.Data!;
        using (new AssertionScope())
        {
            updated.Status.Value.Should().Be(state.UpdateRequest.Status);
            updated.Description.Should().Be(state.UpdateRequest.Description);
            updated.Name.Should().Be(state.CreatedSite.Name);
            updated.Slug.Should().Be(state.CreatedSite.Slug);
        }

        var retrieved = await client.GetSiteAsync(state.CreatedSite.Id);
        retrieved.StatusCode.Should().Be(HttpStatusCode.OK);
        retrieved.Data.Should().NotBeNull();
        retrieved.Data.Should().BeEquivalentTo(updated);
        state.PersistedSite = updated;
    }

    [Then("the Site is unavailable through the service")]
    public async Task VerifyDeletionAsync()
    {
        state.ApiResponses.DeleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var retrieved = await client.GetSiteAsync(state.CreatedSite.Id);
        retrieved.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Then("the Site details are saved")]
    public async Task VerifyPersistenceAsync()
    {
        var site = state.PersistedSite;
        SiteRowDto? row = await database.GetBySlugAsync(site.Slug);
        row.Should().NotBeNull();
        row.Should().BeEquivalentTo(new SiteRowDto
        {
            Id = site.Id,
            Name = site.Name,
            Slug = site.Slug,
            Status = site.Status.Value,
            Description = site.Description
        }, "all represented persisted Site fields must match; Url, Display and status labels are derived");
    }

    [Then("the Site is no longer saved")]
    public async Task VerifyAbsenceAsync() => (await database.GetBySlugAsync(state.CreatedSite.Slug)).Should().BeNull();

    internal static async Task DeleteOwnedSiteAsync(SitesApiClient client, int id)
    {
        RestResponse response = await client.DeleteSiteAsync(id);
        if (response.StatusCode is not (HttpStatusCode.NoContent or HttpStatusCode.NotFound))
        {
            throw new InvalidOperationException($"Failed to clean up site {id}: {response.StatusCode} ({response.ResponseStatus}).");
        }
    }

    private async Task<RestResponse<SiteDetailDto>> CreateOwnedSiteAsync(CreateSiteDto request)
    {
        RestResponse<SiteDetailDto> response = await client.CreateSiteAsync(request);
        if (response.Data is { Id: > 0 } site)
        {
            int id = site.Id;
            cleanup.AddCleanUpAction(() => DeleteOwnedSiteAsync(client, id));
        }

        return response;
    }
}
