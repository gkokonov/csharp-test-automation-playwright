using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Ipam;

namespace CsharpTestAutomation.Bdd.Tests.Context;

public sealed class IpAddressScenarioState
{
    public CreateIpAddressDto CreateRequest { get; set; } = null!;

    public UpdateIpAddressDto UpdateRequest { get; set; } = null!;

    public IpAddressDetailDto CreatedIpAddress { get; set; } = null!;

    public IpAddressDetailDto PersistedIpAddress { get; set; } = null!;

    public ApiResponseState<IpAddressDetailDto> ApiResponses { get; } = new();
}
