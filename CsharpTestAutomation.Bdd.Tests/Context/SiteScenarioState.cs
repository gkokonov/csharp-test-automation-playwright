using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Common;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;
using RestSharp;

namespace CsharpTestAutomation.Bdd.Tests.Context;

/// <summary>Mutable Site state shared only by the bindings of one scenario.</summary>
public sealed class SiteScenarioState
{
    public CreateSiteDto Request { get; set; } = null!;

    public UpdateSiteDto Update { get; set; } = null!;

    public SiteDetailDto Created { get; set; } = null!;

    public SiteDetailDto PersistedSite { get; set; } = null!;

    public RestResponse<SiteDetailDto> DetailResponse { get; set; } = null!;

    public RestResponse<PagedResultDto<SiteDetailDto>> SearchResponse { get; set; } = null!;

    public RestResponse DeleteResponse { get; set; } = null!;

    public SiteDetailsPage DetailsPage { get; set; } = null!;

    public SitesListPage ListPage { get; set; } = null!;
}
