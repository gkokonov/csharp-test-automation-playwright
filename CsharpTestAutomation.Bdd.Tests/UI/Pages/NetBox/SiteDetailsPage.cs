using CsharpTestAutomation.Bdd.Tests.Configurations;
using CsharpTestAutomation.Bdd.Tests.UI.Components.NetBox;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

public sealed class SiteDetailsPage(IPage page) : BaseUIPage(page)
{
    private SiteDeleteConfirmationDialog DeleteConfirmation =>
        new(Page, Page.Locator("#htmx-modal"));

    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true });

    public async Task NavigateAsync(int id)
    {
        Uri siteUri = new(new Uri(ExtendedConfiguration.NetBox.BaseUrl, UriKind.Absolute), $"dcim/sites/{id}/");
        await Page.GotoAsync(siteUri.ToString());
        await WaitUntilLoadedAsync();
    }

    public ILocator Name => AttributeValue("Name");

    // NetBox displays the slug in the page header, outside the attribute table.
    public ILocator Slug => Page.Locator(".page-header code");

    public ILocator Status => AttributeValue("Status");

    public ILocator Description => AttributeValue("Description");

    public async Task<SiteEditPage> EditAsync()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true }).ClickAsync();
        SiteEditPage editPage = GetPage<SiteEditPage>();
        await editPage.WaitUntilLoadedAsync();
        return editPage;
    }

    public async Task<SitesListPage> DeleteAsync()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete", Exact = true }).ClickAsync();
        await DeleteConfirmation.ConfirmAsync();
        SitesListPage sitesPage = GetPage<SitesListPage>();
        await sitesPage.WaitUntilLoadedAsync();
        return sitesPage;
    }

    private ILocator AttributeValue(string attributeName)
    {
        ILocator attributeNameLocator = Page.GetByText(attributeName, new() { Exact = true });
        ILocator attributeRow = Page.GetByRole(AriaRole.Row).Filter(new() { Has = attributeNameLocator });
        return attributeRow.GetByRole(AriaRole.Cell).Last;
    }
}

