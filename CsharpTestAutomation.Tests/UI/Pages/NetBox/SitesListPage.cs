using CsharpTestAutomation.Tests.UI.Components.NetBox;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Pages.NetBox;

public sealed class SitesListPage(IPage page) : BaseUIPage(page)
{
    private SiteDeleteConfirmationDialog DeleteConfirmation =>
        new(Page, Page.Locator("#htmx-modal"));

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

    public async Task DeleteSiteAsync(string slug)
    {
        ILocator siteRow = GetSiteRow(slug);
        await siteRow.GetByRole(AriaRole.Link, new() { Name = "Delete", Exact = true }).ClickAsync();
        await DeleteConfirmation.ConfirmAsync();
        await WaitUntilLoadedAsync();
    }
}
