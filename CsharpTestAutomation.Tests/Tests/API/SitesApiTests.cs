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

    protected override async Task OnSetUpAsync()
    {
        await base.OnSetUpAsync();

        RegisterClient(new SitesApiClient(RestClientFactory, NetBoxAuthenticator));
    }

    [Test]
    [AllureStory("Creating a site returns the created representation")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    public async Task CreateSite_ShouldReturnCreatedSite()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();

        // Act
        RestResponse<SiteDetailDto> response = await SitesClient.CreateSiteAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Data.Should().NotBeNull();

        SiteDetailDto created = response.Data!;
        ScenarioCleanupActions.AddCleanUpAction(() => DeleteSiteAsync(created.Id));

        using (new AssertionScope())
        {
            created.Id.Should().BePositive();
            created.Name.Should().Be(request.Name);
            created.Slug.Should().Be(request.Slug);
            created.Status.Value.Should().Be(request.Status);
            created.Description.Should().Be(request.Description);
        }
    }

    [Test]
    [AllureStory("A created site is persisted and retrievable through both the REST API and PostgreSQL")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    [AllureDescription("Verifies persistence at two layers: a follow-up REST GET must match the create response, and the PostgreSQL row must match the same fields.")]
    public async Task CreateSite_ShouldPersistSiteInDatabase()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SitesClient.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;
        ScenarioCleanupActions.AddCleanUpAction(() => DeleteSiteAsync(created.Id));

        // Act
        RestResponse<SiteDetailDto> getResponse = await SitesClient.GetSiteAsync(created.Id);

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Data.Should().BeEquivalentTo(created);

        SiteRowDto? row = SitesDatabaseRepository.GetBySlug(created.Slug);

        using (new AssertionScope())
        {
            row.Should().NotBeNull();
            row!.Id.Should().Be(created.Id);
            row.Name.Should().Be(created.Name);
            row.Slug.Should().Be(created.Slug);
            row.Status.Should().Be(created.Status.Value);
        }
    }

    [Test]
    [AllureStory("A created site can be found by filtering on its slug")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public async Task GetSite_ShouldReturnMatchingSiteBySlug()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SitesClient.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;
        ScenarioCleanupActions.AddCleanUpAction(() => DeleteSiteAsync(created.Id));

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
    public async Task UpdateSite_ShouldChangeStatus()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SitesClient.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;
        ScenarioCleanupActions.AddCleanUpAction(() => DeleteSiteAsync(created.Id));

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
        getResponse.Data.Should().BeEquivalentTo(updated);
    }

    [Test]
    [AllureStory("Deleting a site removes it so a subsequent lookup returns 404")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    public async Task DeleteSite_ShouldRemoveSite()
    {
        // Arrange
        CreateSiteDto request = new CreateSiteDtoBuilder().Default().Build();
        RestResponse<SiteDetailDto> createResponse = await SitesClient.CreateSiteAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Data.Should().NotBeNull();

        SiteDetailDto created = createResponse.Data!;
        // Registered even though this test performs its own delete, so an early failure still cleans up.
        ScenarioCleanupActions.AddCleanUpAction(() => DeleteSiteAsync(created.Id));

        // Act
        RestResponse deleteResponse = await SitesClient.DeleteSiteAsync(created.Id);

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        RestResponse<SiteDetailDto> getResponse = await SitesClient.GetSiteAsync(created.Id);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Deletes a site, tolerating 404 so it is safe to call as a cleanup action regardless of
    /// whether the test under test already removed the record.
    /// </summary>
    private async Task DeleteSiteAsync(int id)
    {
        RestResponse response = await SitesClient.DeleteSiteAsync(id);

        if (response.StatusCode is not (HttpStatusCode.NoContent or HttpStatusCode.NotFound))
        {
            throw new InvalidOperationException($"Failed to clean up site {id}: received {response.StatusCode}.");
        }
    }
}
