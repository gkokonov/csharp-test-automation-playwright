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
public class GetCpfByIdTests : ApiTestBase
{
    [SetUp]
    public void SetUp() => TestContainer.Register(new CpfAppApiClient(RestClientFactory, BootstrapAuthenticator));

    [Test]
    [AllureStory("GET /api/cpfs/{id} with an existing id returns exact CPF details")]
    public async Task GetCpfById_WithExistingId_ReturnsExactCpfDetails()
    {
        // Arrange
        CpfAppApiClient client = TestContainer.Get<CpfAppApiClient>()!;
        List<CpfRow> all = CpfQueries.SelectAllCpfs();
        CpfRow expected = RequireDbData(all, "No CPF records exist in this environment.");

        // Act
        RestResponse<CpfListItemDto> response = await client.GetCpfByIdAsync(expected.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        // TeamLeadName is excluded: the API resolves it at runtime from Azure AD — not stored in the database.
        expected.Should().BeEquivalentTo(response.Data!, options => options.Excluding(x => x.TeamLeadName));
    }

    [Test]
    [AllureStory("GET /api/cpfs/{id} returns 404 when the CPF does not exist")]
    public async Task GetCpfById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        CpfAppApiClient client = TestContainer.Get<CpfAppApiClient>()!;
        var nonExistentId = Guid.NewGuid();

        // Act
        RestResponse<CpfListItemDto> response = await client.GetCpfByIdAsync(nonExistentId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Data.Should().BeNull();
        // API returns an empty body for 404 — no ProblemDetails payload.
        response.Content.Should().BeNullOrEmpty();
    }

    [Test]
    [AllureStory("GET /api/cpfs/{id} returns 404 when the id is not a valid GUID")]
    public async Task GetCpfById_WithMalformedId_ReturnsNotFound()
    {
        // Act
        CpfAppApiClient client = TestContainer.Get<CpfAppApiClient>()!;
        RestResponse<CpfListItemDto> response = await client.GetCpfByRawIdAsync("not-a-guid");

        // Assert
        // The route constraint requires a GUID — a malformed id causes the route not to match, returning 404.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Data.Should().BeNull();
        response.Content.Should().BeNullOrEmpty();
    }

    [Test]
    [AllureStory("GET /api/cpfs/{id} returns 401 when called without authentication")]
    public async Task GetCpfById_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        CpfAppApiClient client = TestContainer.Get<CpfAppApiClient>()!;
        var anyId = Guid.NewGuid();

        // Act
        // AnonymousAuthenticator suppresses the client-wide bootstrap token for this request only.
        RestResponse<CpfListItemDto> response = await client.GetCpfByIdAsync(anyId, AnonymousAuthenticator.Instance);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }
}
