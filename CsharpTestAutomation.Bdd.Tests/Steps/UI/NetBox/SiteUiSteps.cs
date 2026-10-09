using System.Net;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CsharpTestAutomation.Bdd.Tests.Api.Clients;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Bdd.Tests.Context;
using CsharpTestAutomation.Bdd.Tests.Steps.Api.NetBox;
using CsharpTestAutomation.Bdd.Tests.UI.Pages;
using CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;
using CsharpTestAutomation.Framework.Common;
using Microsoft.Playwright;
using Reqnroll;
using static Microsoft.Playwright.Assertions;

namespace CsharpTestAutomation.Bdd.Tests.Steps.UI.NetBox;

[Binding]
[Scope(Tag = "UI")]
public sealed class SiteUiSteps(IPage page, SitesApiClient client, ScenarioCleanupActions cleanup, SiteScenarioState state)
{
    [Given("an authenticated administrator")]
    public async Task VerifyAuthenticationAsync()
    {
        var list = BaseUIPage.Create<SitesListPage>(page);
        await list.NavigateAsync();
        await BaseUIPage.Create<NetBoxLoginPage>(page).WaitUntilSignedInAsync();
    }

    [When("the Site is created in the web application")]
    public async Task CreateAsync()
    {
        RegisterUiSiteCleanup(state.Request.Slug);
        var list = BaseUIPage.Create<SitesListPage>(page);
        await list.NavigateAsync();
        SiteEditPage edit = await list.AddSiteAsync();
        state.DetailsPage = await edit.CreateSiteAsync(state.Request);
        if (Environment.GetEnvironmentVariable("BDD_VALIDATE_UI_SUBMISSION_FAILURE") == "1")
        {
            throw new AssertionException("Injected failure after UI submission and before API identification.");
        }
    }

    [When("the Site is edited in the web application")]
    public async Task UpdateAsync()
    {
        var details = BaseUIPage.Create<SiteDetailsPage>(page);
        await details.NavigateAsync(state.Created.Id);
        SiteEditPage edit = await details.EditAsync();
        state.DetailsPage = await edit.UpdateAsync(state.Update.Status, state.Update.Description);
    }

    [When("the Site is deleted in the web application")]
    public async Task DeleteAsync()
    {
        var details = BaseUIPage.Create<SiteDetailsPage>(page);
        await details.NavigateAsync(state.Created.Id);
        state.ListPage = await details.DeleteAsync();
    }

    [Then("the requested Site details are displayed and retrievable")]
    public async Task VerifyCreationAsync()
    {
        string name = await state.DetailsPage.GetNameAsync();
        string slug = await state.DetailsPage.GetSlugAsync();
        string status = await state.DetailsPage.GetStatusAsync();
        string description = await state.DetailsPage.GetDescriptionAsync();
        using (new AssertionScope())
        {
            name.Should().Be(state.Request.Name);
            slug.Should().Be(state.Request.Slug);
            status.Should().BeEquivalentTo(state.Request.Status);
            description.Should().Be(state.Request.Description);
        }

        var response = await client.FindSitesBySlugAsync(state.Request.Slug);
        // Register a usable ID before any response assertions; the slug fallback already exists.
        foreach (SiteDetailDto owned in response.Data?.Results.Where(x => x.Slug == state.Request.Slug && x.Id > 0) ?? [])
        {
            int id = owned.Id;
            cleanup.AddCleanUpAction(() => SiteApiSteps.DeleteOwnedSiteAsync(client, id));
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Count.Should().Be(1);
        SiteDetailDto site = response.Data.Results.Should().ContainSingle().Which;
        using (new AssertionScope())
        {
            site.Name.Should().Be(state.Request.Name);
            site.Slug.Should().Be(state.Request.Slug);
            site.Status.Value.Should().Be(state.Request.Status);
            site.Description.Should().Be(state.Request.Description);
        }

        state.PersistedSite = site;
    }

    [Then("the application displays and saves the changes and retains the other Site details")]
    public async Task VerifyUpdateAsync()
    {
        string status = await state.DetailsPage.GetStatusAsync();
        string description = await state.DetailsPage.GetDescriptionAsync();
        using (new AssertionScope())
        {
            status.Should().BeEquivalentTo(state.Update.Status);
            description.Should().Be(state.Update.Description);
        }

        var response = await client.GetSiteAsync(state.Created.Id);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        SiteDetailDto site = response.Data!;
        using (new AssertionScope())
        {
            site.Status.Value.Should().Be(state.Update.Status);
            site.Description.Should().Be(state.Update.Description);
            site.Name.Should().Be(state.Created.Name);
            site.Slug.Should().Be(state.Created.Slug);
        }

        state.PersistedSite = site;
    }

    [Then("the Site is absent from the application and service")]
    public async Task VerifyDeletionAsync()
    {
        await Expect(state.ListPage.GetSiteRow(state.Created.Name)).Not.ToBeVisibleAsync();
        (await state.ListPage.ContainsSiteAsync(state.Created.Name)).Should().BeFalse();
        var response = await client.GetSiteAsync(state.Created.Id);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Register before submission so later navigation or assertion failures cannot skip cleanup.
    private void RegisterUiSiteCleanup(string slug) => cleanup.AddCleanUpAction(async () =>
    {
        var response = await client.FindSitesBySlugAsync(slug);
        if (response.StatusCode != HttpStatusCode.OK || response.Data is null)
        {
            throw new InvalidOperationException($"Could not find the UI-created site for cleanup: {response.StatusCode} ({response.ResponseStatus}).");
        }

        foreach (SiteDetailDto site in response.Data.Results.Where(x => x.Slug == slug))
        {
            await SiteApiSteps.DeleteOwnedSiteAsync(client, site.Id);
        }
    });
}
