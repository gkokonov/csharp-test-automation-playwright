using CsharpTestAutomation.Bdd.Tests.UI.Pages;
using CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;
using Microsoft.Playwright;
using Reqnroll;

namespace CsharpTestAutomation.Bdd.Tests.Steps.UI.NetBox;

[Binding]
[Scope(Tag = "UI")]
public sealed class NetBoxUiSteps(IPage page)
{
    [Given("an authenticated administrator")]
    public async Task VerifyAuthenticationAsync()
    {
        await BaseUIPage.Create<SitesListPage>(page).NavigateAsync();
        await BaseUIPage.Create<NetBoxLoginPage>(page).WaitUntilSignedInAsync();
    }
}
