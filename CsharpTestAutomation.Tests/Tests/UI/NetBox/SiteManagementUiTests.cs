using System.Net;
using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CsharpTestAutomation.Tests.Api.Clients;
using CsharpTestAutomation.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Tests.Api.Factories;
using CsharpTestAutomation.Tests.Database.NetBox.DTO;
using CsharpTestAutomation.Tests.Database.NetBox.Queries;
using CsharpTestAutomation.Tests.Steps.Api.NetBox;
using CsharpTestAutomation.Tests.UI.Pages.NetBox;
using RestSharp;
using static Microsoft.Playwright.Assertions;

namespace CsharpTestAutomation.Tests.Tests.UI.NetBox;

/// <summary>
/// Site CRUD coverage driven through the NetBox web UI with cross-layer REST API and PostgreSQL
/// verification. Derives from <see cref="NetBoxUiTestBase"/> for authenticated storage state and
/// API client lifecycle management.
/// </summary>
[AllureSuite("UI")]
[AllureEpic("NetBox")]
[AllureFeature("Site Management")]
public class SiteManagementUiTests : NetBoxUiTestBase
{
    private SitesApiClient SitesClient => GetClient<SitesApiClient>();

    private SiteSteps SiteSteps => new(SitesClient, ScenarioCleanupActions);

    protected override async Task OnSetUpAsync()
    {
        await base.OnSetUpAsync();

        RegisterClient(new SitesApiClient(RestClientFactory, NetBoxAuthenticator));
    }

    [Test]
    [AllureStory("Creating a site through the UI persists across UI, REST API, and PostgreSQL layers")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    public async Task Verify_SitePersistedAcrossLayers_When_SiteIsCreatedThroughUi()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        SiteSteps.RegisterUiSiteCleanup(request.Slug);

        // Act
        SitesListPage listPage = GetPage<SitesListPage>();
        await listPage.NavigateAsync();
        SiteEditPage editPage = await listPage.AddSiteAsync();
        SiteDetailsPage detailsPage = await editPage.CreateSiteAsync(request);

        // Assert
        var uiName = await detailsPage.GetNameAsync();
        var uiSlug = await detailsPage.GetSlugAsync();
        var uiStatus = await detailsPage.GetStatusAsync();
        var uiDescription = await detailsPage.GetDescriptionAsync();

        using (new AssertionScope())
        {
            uiName.Should().Be(request.Name);
            uiSlug.Should().Be(request.Slug);
            uiStatus.Should().BeEquivalentTo(request.Status);
            uiDescription.Should().Be(request.Description);
        }

        RestResponse<PagedResultDto<SiteDetailDto>> apiResponse = await SitesClient.FindSitesBySlugAsync(request.Slug);
        apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Count.Should().Be(1);

        SiteDetailDto siteFromApi = apiResponse.Data.Results.Single();
        SiteSteps.RegisterSiteCleanup(siteFromApi.Id);

        using (new AssertionScope())
        {
            siteFromApi.Name.Should().Be(request.Name);
            siteFromApi.Slug.Should().Be(request.Slug);
            siteFromApi.Status.Value.Should().Be(request.Status);
            siteFromApi.Description.Should().Be(request.Description);
        }

        AssertPersistedSite(siteFromApi);
    }

    [Test]
    [AllureStory("Updating a site through the UI reflects the changed status across UI, REST API, and PostgreSQL layers")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public async Task Verify_SiteStatusChangedAcrossLayers_When_SiteIsUpdatedThroughUi()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SiteSteps.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;

        UpdateSiteDto update = new UpdateSiteDtoBuilder().Default().Build();

        // Act
        SiteDetailsPage detailsPage = GetPage<SiteDetailsPage>();
        await detailsPage.NavigateAsync(created.Id);
        SiteEditPage editPage = await detailsPage.EditAsync();
        detailsPage = await editPage.UpdateAsync(update.Status, update.Description);

        // Assert
        var uiStatus = await detailsPage.GetStatusAsync();
        var uiDescription = await detailsPage.GetDescriptionAsync();

        using (new AssertionScope())
        {
            uiStatus.Should().BeEquivalentTo(update.Status);
            uiDescription.Should().Be(update.Description);
        }

        RestResponse<SiteDetailDto> getResponse = await SitesClient.GetSiteAsync(created.Id);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Data.Should().NotBeNull();

        using (new AssertionScope())
        {
            getResponse.Data!.Status.Value.Should().Be(update.Status);
            getResponse.Data.Description.Should().Be(update.Description);
            getResponse.Data.Name.Should().Be(created.Name);
            getResponse.Data.Slug.Should().Be(created.Slug);
        }

        AssertPersistedSite(getResponse.Data!);
    }

    [Test]
    [AllureStory("Deleting a site through the UI removes it across UI, REST API, and PostgreSQL layers")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    public async Task Verify_SiteRemovedAcrossLayers_When_SiteIsDeletedThroughUi()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SiteSteps.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;

        // Act
        SiteDetailsPage detailsPage = GetPage<SiteDetailsPage>();
        await detailsPage.NavigateAsync(created.Id);
        SitesListPage listPage = await detailsPage.DeleteAsync();

        // Assert
        await Expect(listPage.GetSiteRow(created.Name)).Not.ToBeVisibleAsync();
        var rowPresent = await listPage.ContainsSiteAsync(created.Name);
        rowPresent.Should().BeFalse();

        RestResponse<SiteDetailDto> getResponse = await SitesClient.GetSiteAsync(created.Id);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        SiteRowDto? row = SitesDatabaseRepository.GetBySlug(created.Slug);
        row.Should().BeNull();
    }

    private static void AssertPersistedSite(SiteDetailDto site)
    {
        SiteRowDto? row = SitesDatabaseRepository.GetBySlug(site.Slug);
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
}
