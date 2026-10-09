using Microsoft.Playwright;

namespace CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

public sealed class SitesListPage(IPage page) : BaseUIPage(page)
{
    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Sites", Exact = true });

    public async Task NavigateAsync()
    {
        Uri sitesUri = new(new Uri(ExtendedConfiguration.NetBox.BaseUrl, UriKind.Absolute), "dcim/sites/");
        await Page.GotoAsync(sitesUri.ToString());
        await WaitUntilLoadedAsync();
    }

    public async Task<SiteEditPage> AddSiteAsync()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();
        SiteEditPage editPage = GetPage<SiteEditPage>();
        await editPage.WaitUntilLoadedAsync();
        return editPage;
    }

    public ILocator GetSiteRow(string nameOrSlug) =>
        Page.GetByRole(AriaRole.Row).Filter(new() { HasText = nameOrSlug });

    public async Task<bool> ContainsSiteAsync(string nameOrSlug) =>
        await GetSiteRow(nameOrSlug).CountAsync() > 0;
}
