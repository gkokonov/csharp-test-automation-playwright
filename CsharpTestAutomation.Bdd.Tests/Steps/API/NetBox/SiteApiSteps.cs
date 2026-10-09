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
    public void PrepareUniqueSite() => state.Request = new CreateSiteDtoBuilder().Default().Build();

    [Given("an owned active Site")]
    public async Task CreatePrerequisiteAsync()
    {
        PrepareUniqueSite();
        var response = await CreateOwnedSiteAsync(state.Request);
        response.StatusCode.Should().Be(HttpStatusCode.Created, "the scenario requires an owned Site");
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().BePositive();
        state.Created = response.Data;
    }

    [Given("valid changes to its status and description")]
    public void PrepareUpdate() => state.Update = new UpdateSiteDtoBuilder().Default().Build();

    [When("the Site is created through the service")]
    public async Task CreateAsync() => state.DetailResponse = await CreateOwnedSiteAsync(state.Request);

    [When("the Site is retrieved through the service")]
    public async Task RetrieveAsync() => state.DetailResponse = await client.GetSiteAsync(state.Created.Id);

    [When("the Site is found by its slug")]
    public async Task FindAsync() => state.SearchResponse = await client.FindSitesBySlugAsync(state.Created.Slug);

    [When("the Site changes are submitted through the service")]
    public async Task UpdateAsync() => state.DetailResponse = await client.UpdateSiteAsync(state.Created.Id, state.Update);

    [When("the Site is deleted through the service")]
    public async Task DeleteAsync() => state.DeleteResponse = await client.DeleteSiteAsync(state.Created.Id);

    [Then("the requested Site details are returned")]
    public void VerifyCreatedDetails()
    {
        state.DetailResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        state.DetailResponse.Data.Should().NotBeNull();
        SiteDetailDto site = state.DetailResponse.Data!;
        state.PersistedSite = site;
        using (new AssertionScope())
        {
            site.Id.Should().BePositive();
            site.Name.Should().Be(state.Request.Name);
            site.Slug.Should().Be(state.Request.Slug);
            site.Status.Value.Should().Be(state.Request.Status);
            site.Description.Should().Be(state.Request.Description);
        }
    }

    [Then("the original Site details are returned")]
    public void VerifyRetrievedDetails()
    {
        state.DetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        state.DetailResponse.Data.Should().NotBeNull();
        state.DetailResponse.Data.Should().BeEquivalentTo(state.Created);
        state.PersistedSite = state.Created;
    }

    [Then("only the expected Site is returned")]
    public void VerifySearch()
    {
        state.SearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        state.SearchResponse.Data.Should().NotBeNull();
        state.SearchResponse.Data!.Count.Should().Be(1);
        state.SearchResponse.Data.Results.Should().ContainSingle().Which.Should().BeEquivalentTo(state.Created);
    }

    [Then("the service saves the changes and retains the other Site details")]
    public async Task VerifyUpdateAsync()
    {
        state.DetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        state.DetailResponse.Data.Should().NotBeNull();
        SiteDetailDto updated = state.DetailResponse.Data!;
        using (new AssertionScope())
        {
            updated.Status.Value.Should().Be(state.Update.Status);
            updated.Description.Should().Be(state.Update.Description);
            updated.Name.Should().Be(state.Created.Name);
            updated.Slug.Should().Be(state.Created.Slug);
        }

        var retrieved = await client.GetSiteAsync(state.Created.Id);
        retrieved.StatusCode.Should().Be(HttpStatusCode.OK);
        retrieved.Data.Should().NotBeNull();
        retrieved.Data.Should().BeEquivalentTo(updated);
        state.PersistedSite = updated;
    }

    [Then("the Site is unavailable through the service")]
    public async Task VerifyDeletionAsync()
    {
        state.DeleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var retrieved = await client.GetSiteAsync(state.Created.Id);
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
    public async Task VerifyAbsenceAsync() => (await database.GetBySlugAsync(state.Created.Slug)).Should().BeNull();

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
