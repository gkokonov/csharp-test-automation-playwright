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
using RestSharp;

namespace CsharpTestAutomation.Tests.Tests.Api;

/// <summary>
/// Site CRUD coverage against the live NetBox REST API. Derives from <see cref="ApiTestBase"/> for
/// its shared <c>RestClientFactory</c>/<c>NetBoxAuthenticator</c> and <c>ScenarioCleanupActions</c>
/// wiring — every API test fixture in this project derives from it.
/// </summary>
[AllureSuite("API")]
[AllureFeature("Site Management")]
public class SitesApiTests : ApiTestBase
{
    private SitesApiClient SitesClient => GetClient<SitesApiClient>();

    private SiteSteps SiteSteps => new(SitesClient, ScenarioCleanupActions);

    protected override async Task OnSetUpAsync()
    {
        await base.OnSetUpAsync();

        RegisterClient(new SitesApiClient(RestClientFactory, NetBoxAuthenticator));
    }

    [Test]
    [AllureStory("Creating a site returns the created representation")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    public async Task Verify_CreatedSiteReturned()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();

        // Act
        RestResponse<SiteDetailDto> response = await SiteSteps.CreateSiteAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Data.Should().NotBeNull();

        SiteDetailDto created = response.Data!;

        using (new AssertionScope())
        {
            created.Id.Should().BePositive();
            created.Name.Should().Be(request.Name);
            created.Slug.Should().Be(request.Slug);
            created.Status.Value.Should().Be(request.Status);
            created.Description.Should().Be(request.Description);
        }

        AssertPersistedSite(created);
    }

    [Test]
    [AllureStory("A created site is persisted and retrievable through both the REST API and PostgreSQL")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    [AllureDescription("Verifies persistence at two layers: a follow-up REST GET must match the create response, and the PostgreSQL row must match the same fields.")]
    public async Task Verify_SitePersistedInDatabase()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SiteSteps.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;

        // Act
        RestResponse<SiteDetailDto> getResponse = await SitesClient.GetSiteAsync(created.Id);

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Data.Should().BeEquivalentTo(created);

        AssertPersistedSite(created);
    }

    [Test]
    [AllureStory("A created site can be found by filtering on its slug")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public async Task Verify_MatchingSiteReturned_When_SiteIsFoundBySlug()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SiteSteps.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;

        // Act
        RestResponse<PagedResultDto<SiteDetailDto>> response = await SitesClient.FindSitesBySlugAsync(request.Slug);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Count.Should().Be(1);
        response.Data.Results.Should().ContainSingle().Which.Should().BeEquivalentTo(created);
    }

    [Test]
    [AllureStory("Updating a site's status and description leaves its other fields unchanged")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public async Task Verify_SiteStatusChanged_When_SiteIsUpdated()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SiteSteps.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;

        UpdateSiteDto update = new UpdateSiteDtoBuilder().Default().Build();

        // Act
        RestResponse<SiteDetailDto> updateResponse = await SitesClient.UpdateSiteAsync(created.Id, update);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updateResponse.Data.Should().NotBeNull();

        SiteDetailDto updated = updateResponse.Data!;

        using (new AssertionScope())
        {
            updated.Status.Value.Should().Be(update.Status);
            updated.Description.Should().Be(update.Description);
            updated.Name.Should().Be(created.Name);
            updated.Slug.Should().Be(created.Slug);
        }

        RestResponse<SiteDetailDto> getResponse = await SitesClient.GetSiteAsync(created.Id);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Data.Should().BeEquivalentTo(updated);
        AssertPersistedSite(updated);
    }

    [Test]
    [AllureStory("Deleting a site removes it so a subsequent lookup returns 404")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    public async Task Verify_SiteRemoved_When_SiteIsDeleted()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SiteSteps.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;

        // Act
        RestResponse deleteResponse = await SitesClient.DeleteSiteAsync(created.Id);

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        RestResponse<SiteDetailDto> getResponse = await SitesClient.GetSiteAsync(created.Id);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        SitesDatabaseRepository.GetBySlug(created.Slug).Should().BeNull();
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
