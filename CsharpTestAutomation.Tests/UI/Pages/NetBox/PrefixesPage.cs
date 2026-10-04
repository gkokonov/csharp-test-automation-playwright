using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Pages.NetBox;

public sealed class PrefixesPage(IPage page) : BaseUIPage(page)
{
    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Prefixes", Exact = true });

    public async Task NavigateAsync()
    {
        Uri uri = new(new Uri(ExtendedConfiguration.NetBox.BaseUrl, UriKind.Absolute), "ipam/prefixes/");
        await Page.GotoAsync(uri.ToString());
        await WaitUntilLoadedAsync();
    }

    public async Task<PrefixEditPage> AddPrefixAsync()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).ClickAsync();
        PrefixEditPage editPage = GetPage<PrefixEditPage>();
        await editPage.WaitUntilLoadedAsync();
        return editPage;
    }
}
