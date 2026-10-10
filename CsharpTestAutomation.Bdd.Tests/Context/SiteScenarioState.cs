using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

namespace CsharpTestAutomation.Bdd.Tests.Context;

/// <summary>Mutable Site state shared only by the bindings of one scenario.</summary>
public sealed class SiteScenarioState
{
    public CreateSiteDto CreateRequest { get; set; } = null!;

    public UpdateSiteDto UpdateRequest { get; set; } = null!;

    public SiteDetailDto CreatedSite { get; set; } = null!;

    public SiteDetailDto PersistedSite { get; set; } = null!;

    public ApiResponseState<SiteDetailDto> ApiResponses { get; } = new();

    public SiteDetailsPage DetailsPage { get; set; } = null!;

    public SitesListPage ListPage { get; set; } = null!;
}
