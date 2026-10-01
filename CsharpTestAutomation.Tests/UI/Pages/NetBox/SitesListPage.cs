using CsharpTestAutomation.Tests.UI.Components.NetBox;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Pages.NetBox;

public sealed class SitesListPage(IPage page) : BaseUIView(page)
{
    private SiteDeleteConfirmationDialog DeleteConfirmation =>
        new(Page, Page.GetByRole(AriaRole.Dialog));

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

    public async Task DeleteSiteAsync(string slug)
    {
        ILocator siteRow = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = slug });
        await siteRow.GetByRole(AriaRole.Link, new() { Name = "Delete", Exact = true }).ClickAsync();
        await DeleteConfirmation.ConfirmAsync();
        await WaitUntilLoadedAsync();
    }
}
