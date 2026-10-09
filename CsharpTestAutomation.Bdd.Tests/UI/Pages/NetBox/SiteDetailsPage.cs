using System.Text.RegularExpressions;
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

    public Task<string> GetNameAsync() => GetAttributeValueAsync("Name");

    public async Task<string> GetSlugAsync()
    {
        ILocator codeLocator = Page.Locator(".page-header code");
        if (await codeLocator.CountAsync() > 0)
        {
            string text = await codeLocator.InnerTextAsync();
            Match match = Regex.Match(text, @"\(([^)]+)\)");
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return await GetAttributeValueAsync("Slug");
    }

    public Task<string> GetStatusAsync() => GetAttributeValueAsync("Status");

    public Task<string> GetDescriptionAsync() => GetAttributeValueAsync("Description");

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

    private async Task<string> GetAttributeValueAsync(string attributeName)
    {
        ILocator attributeNameLocator = Page.GetByText(attributeName, new() { Exact = true });
        ILocator attributeRow = Page.GetByRole(AriaRole.Row).Filter(new() { Has = attributeNameLocator });
        string text = await attributeRow.GetByRole(AriaRole.Cell).Last.InnerTextAsync();
        return text.Trim();
    }
}

