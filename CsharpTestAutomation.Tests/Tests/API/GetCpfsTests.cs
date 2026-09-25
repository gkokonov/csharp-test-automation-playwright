using System.Net;
using Allure.NUnit.Attributes;
using AwesomeAssertions;
using CsharpTestAutomation.Framework.API.Authentication;
using CsharpTestAutomation.Tests.Api.Clients;
using CsharpTestAutomation.Tests.Api.Dtos;
using CsharpTestAutomation.Tests.Database.CPF.DTO;
using CsharpTestAutomation.Tests.Database.CPF.Queries;
using RestSharp;

namespace CsharpTestAutomation.Tests.Tests.Api;

[AllureSuite("API")]
[AllureFeature("CPFs")]
public class GetCpfsTests : ApiTestBase
{
    [SetUp]
    public void SetUp() => TestContainer.Register(new CpfAppApiClient(RestClientFactory, BootstrapAuthenticator));

    [Test]
    [AllureStory("GET /api/cpfs returns CPFs matching the cpfs database table")]
    public async Task GetCpfs_ReturnsCpfsMatchingDatabaseTable()
    {
        // Arrange
        CpfAppApiClient client = TestContainer.Get<CpfAppApiClient>()!;
        List<CpfRow> expected = CpfQueries.SelectAllCpfs();

        // Act
        RestResponse<List<CpfListItemDto>> response = await client.GetCpfsAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        // TeamLeadName is excluded: the API resolves it at runtime from Azure AD — not stored in the database.
        expected.Should().BeEquivalentTo(response.Data!, options => options.Excluding(x => x.TeamLeadName));
    }

    [Test]
    [AllureStory("GET /api/cpfs with searchTerm returns a matching CPF in the result list")]
    public async Task GetCpfs_WithMatchingSearchTerm_ReturnsMatchingCpfInList()
    {
        // Arrange
        CpfAppApiClient client = TestContainer.Get<CpfAppApiClient>()!;
        List<CpfRow> all = CpfQueries.SelectAllCpfs();
        all.Should().NotBeEmpty();
        CpfRow expected = all[Random.Shared.Next(all.Count)];

        // Act
        RestResponse<List<CpfListItemDto>> response = await client.GetCpfsAsync(expected.Title);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull().And.Contain(x => x.Id == expected.Id);
        CpfListItemDto match = response.Data!.Single(x => x.Id == expected.Id);
        // TeamLeadName is excluded: the API resolves it at runtime from Azure AD — not stored in the database.
        expected.Should().BeEquivalentTo(match, options => options.Excluding(x => x.TeamLeadName));
    }

    [Test]
    [AllureStory("GET /api/cpfs returns an empty result when the search term matches no CPF")]
    public async Task GetCpfs_WithUnmatchedSearchTerm_ReturnsEmptyResult()
    {
        // Act
        CpfAppApiClient client = TestContainer.Get<CpfAppApiClient>()!;
        RestResponse<List<CpfListItemDto>> response = await client.GetCpfsAsync($"zzz-no-match-{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    [AllureStory("GET /api/cpfs returns 401 when called without authentication")]
    public async Task GetCpfs_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        CpfAppApiClient client = TestContainer.Get<CpfAppApiClient>()!;
        // AnonymousAuthenticator suppresses the client-wide bootstrap token for this request only.
        RestResponse<List<CpfListItemDto>> response = await client.GetCpfsAsync(AnonymousAuthenticator.Instance);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }
}
