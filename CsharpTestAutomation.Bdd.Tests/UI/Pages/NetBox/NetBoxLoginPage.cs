using CsharpTestAutomation.Bdd.Tests.Configurations;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

public sealed class NetBoxLoginPage(IPage page) : BaseUIPage(page)
{
    private ILocator Username => Page.GetByLabel("Username");

    private ILocator Password => Page.GetByLabel("Password");

    private ILocator SignInButton => Page.GetByRole(AriaRole.Button, new() { Name = "Sign In", Exact = true });

    protected override ILocator PageReadyLocator => Page.GetByRole(AriaRole.Heading, new() { Name = "Log In", Exact = true });

    public async Task OpenAsync()
    {
        await Page.GotoAsync(ExtendedConfiguration.NetBox.BaseUrl);
        await WaitUntilLoadedAsync();
    }

    public async Task SignInAsync(string username, string password)
    {
        await Username.FillAsync(username);
        await Password.FillAsync(password);
        await SignInButton.ClickAsync();
        await WaitUntilSignedInAsync();
    }

    public Task WaitUntilSignedInAsync() =>
        Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Open user menu", Exact = true })).ToBeVisibleAsync();
}
