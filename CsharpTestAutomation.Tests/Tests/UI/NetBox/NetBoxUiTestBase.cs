using CsharpTestAutomation.Tests.Tests.UI;

namespace CsharpTestAutomation.Tests.Tests.UI.NetBox;

public abstract class NetBoxUiTestBase : UiTestBase
{
    protected override async Task OnSetUpAsync() =>
        await InitializePlaywrightEnvironmentAsync(NetBoxUiSetupFixture.StorageStatePath);
}
