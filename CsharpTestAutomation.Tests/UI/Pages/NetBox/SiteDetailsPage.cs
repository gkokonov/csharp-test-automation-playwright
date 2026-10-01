using CsharpTestAutomation.Tests.UI.Components.NetBox;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Pages.NetBox;

public sealed class SiteDetailsPage(IPage page) : BaseUIView(page)
{
    private SiteDeleteConfirmationDialog DeleteConfirmation =>
        new(Page, Page.GetByRole(AriaRole.Dialog));

    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true });

    public Task<string> GetNameAsync() => GetAttributeValueAsync("Name");

    public Task<string> GetSlugAsync() => GetAttributeValueAsync("Slug");

    public Task<string> GetStatusAsync() => GetAttributeValueAsync("Status");

    public Task<string> GetDescriptionAsync() => GetAttributeValueAsync("Description");

    public async Task<SiteEditPage> EditAsync()
    {
        await Page.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }).ClickAsync();
        SiteEditPage editPage = GetPage<SiteEditPage>();
        await editPage.WaitUntilLoadedAsync();
        return editPage;
    }

    public async Task<SitesListPage> DeleteAsync()
    {
        await Page.GetByRole(AriaRole.Link, new() { Name = "Delete", Exact = true }).ClickAsync();
        await DeleteConfirmation.ConfirmAsync();
        SitesListPage sitesPage = GetPage<SitesListPage>();
        await sitesPage.WaitUntilLoadedAsync();
        return sitesPage;
    }

    private Task<string> GetAttributeValueAsync(string attributeName)
    {
        ILocator attributeNameLocator = Page.GetByText(attributeName, new() { Exact = true });
        ILocator attributeRow = Page.GetByRole(AriaRole.Row).Filter(new() { Has = attributeNameLocator });
        return attributeRow.GetByRole(AriaRole.Cell).Last.InnerTextAsync();
    }
}
