using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Devices;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Bdd.Tests.Steps.Api.NetBox;
using CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

namespace CsharpTestAutomation.Bdd.Tests.Context;

/// <summary>Mutable Device state shared only by the bindings of one scenario.</summary>
public sealed class DeviceScenarioState
{
    public DevicePrerequisites Prerequisites { get; set; } = null!;
    public CreateDeviceDto Request { get; set; } = null!;
    public UpdateDeviceDto Update { get; set; } = null!;
    public SiteDetailDto? OtherSite { get; set; }
    public SiteDetailDto ExpectedSite { get; set; } = null!;
    public DeviceDetailDto? Created { get; set; }
    public DeviceDetailDto PersistedDevice { get; set; } = null!;
    public List<DeviceDetailDto> OwnedDevices { get; } = [];
    public ApiResponseState<DeviceDetailDto> Api { get; } = new();
    public DeviceDetailsPage DetailsPage { get; set; } = null!;
}
