using System.Net;
using Allure.NUnit.Attributes;
using AwesomeAssertions;
using CsharpTestAutomation.Tests.Api.Clients;
using CsharpTestAutomation.Tests.Api.Dtos;
using CsharpTestAutomation.Tests.Api.Factories;
using CsharpTestAutomation.Tests.Database.CPF.DTO;
using CsharpTestAutomation.Tests.Database.CPF.Queries;
using RestSharp;

namespace CsharpTestAutomation.Tests.Tests.Api;

[AllureSuite("API")]
[AllureFeature("CPFs")]
public class CreateCpfTests : ApiTestBase
{
    // Override to pin a specific ISO 3166-1 alpha-2 code; empty means pick one from the DB.
    private const string CountryCode = "";

    // Pinned to Teodora Metodieva Anachkova Kokonova. The id must resolve in the identity provider,
    // otherwise the API flags the created team member for deletion and it would not round-trip back
    // as the CPF's team lead.
    // Alternate identity-resolvable team lead:
    // "65959e60-fa75-4de7-9c51-2e3525d36f04" -> Srinivasa Rao Katragadda
    // "dd0f9d09-4868-4e08-bb4b-10b10bc33323"-> Deepika Prakash
    private const string TeamLeadId = "18001f38-b502-476c-b648-ff2b04305486";

    [SetUp]
    public void SetUp() => TestContainer.Register(new CpfAppApiClient(RestClientFactory, BootstrapAuthenticator));

    [Test]
    [AllureStory("POST /api/cpfs with a valid payload creates a CPF persisted in the database")]
    public async Task CreateCpf_WithValidPayload_PersistsCpfInDatabase()
    {
        // Arrange
        CpfAppApiClient client = TestContainer.Get<CpfAppApiClient>()!;
        (var countryCode, var countryName) = ResolveCountry();
        CreateCpfDto request = new CreateCpfDtoBuilder()
            .Default()
            .With(x => x.CountryCode = countryCode)
            .With(x => x.TeamLeadId = TeamLeadId)
            .Build();

        // Act
        RestResponse<Guid> response = await client.CreateCpfAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Data.Should().NotBe(Guid.Empty);
        Guid createdId = response.Data;
        ScenarioCleanupActions.AddCleanUpAction(async () => await client.DeleteCpfAndExpectNoContentAsync(createdId));

        // A missing created row is a failed persist, not missing seed data.
        CpfRow? persisted = CpfQueries.SelectCpfById(createdId);
        persisted.Should().NotBeNull("the created CPF must be readable from the database");
        CpfRow created = persisted!;
        created.PCode.Should().NotBeNullOrWhiteSpace();

        CpfRow expected = new(
            Id: createdId,
            PCode: null,
            Title: request.Title,
            Description: request.Description,
            CountryCode: request.CountryCode,
            CountryName: countryName,
            CoverageStartYear: request.CoverageStartYear,
            CoverageEndYear: request.CoverageEndYear,
            Stage: "Preparation",
            TeamLeadId: TeamLeadId);

        created.Should().BeEquivalentTo(expected, options => options
            // PCode is a server-generated sequence (e.g. P700001) — not known before creation.
            .Excluding(x => x.PCode));
    }

    private static (string CountryCode, string? CountryName) ResolveCountry()
    {
        List<CpfRow> cpfs = CpfQueries.SelectAllCpfs();
        if (!string.IsNullOrWhiteSpace(CountryCode))
        {
            CpfRow pinned = cpfs.FirstOrDefault(r => r.CountryCode == CountryCode)
                ?? throw new InvalidOperationException($"No CPF row has country code '{CountryCode}'.");
            return (pinned.CountryCode, pinned.CountryName);
        }

        CpfRow row = RequireDbData(cpfs, "No CPF records exist to resolve a country code.");
        return (row.CountryCode, row.CountryName);
    }

}
