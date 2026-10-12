using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Ipam;
using CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

namespace CsharpTestAutomation.Bdd.Tests.Context;

public sealed class PrefixScenarioState
{
    public CreatePrefixDto CreateRequest { get; set; } = null!;

    public UpdatePrefixDto UpdateRequest { get; set; } = null!;

    public PrefixDetailDto CreatedPrefix { get; set; } = null!;

    public ApiResponseState<PrefixDetailDto> ApiResponses { get; } = new();

    public PrefixDetailsPage DetailsPage { get; set; } = null!;
}
