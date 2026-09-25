using Allure.NUnit.Attributes;
using AwesomeAssertions;
using CsharpTestAutomation.Tests.Authentication;

namespace CsharpTestAutomation.Tests.Tests.UI;

[AllureSuite("UI")]
[AllureFeature("UI-Bootstrapped Authentication")]
[Category("AuthBootstrap")]
public class UiBootstrapUiTests : UiTestBase
{
    protected override async Task OnSetUpAsync() => await InitializeAuthenticatedPlaywrightEnvironmentAsync(BootstrapSession.Default);

    [Test]
    [AllureStory("Saved storage-state opens CPF app as authenticated user")]
    public async Task OpenCpfApp_WithBootstrappedStorageState_IsAuthenticated()
    {
        await Page.GotoAsync(s_configuration.Ui!.ApplicationUrl!);

        Page.Url.Should().NotContain("login.microsoftonline.com", "saved storage state should bypass Microsoft sign-in");
        Page.Url.Should().NotContain("wbstsp.worldbank.org", "saved storage state should bypass federated sign-in");
    }
}

