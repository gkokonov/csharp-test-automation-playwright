using CsharpTestAutomation.Tests.Api.Dtos.Sites;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Pages.NetBox;

public sealed class SiteEditPage(IPage page) : BaseUIView(page)
{
    private ILocator Name => Page.GetByLabel("Name");

    private ILocator Slug => Page.GetByLabel("Slug");

    // NetBox's accessible combobox is a hidden helper input; the form value is held by this select.
    private ILocator Status => Page.Locator("select[name='status']");

    private ILocator Description => Page.GetByLabel("Description");

    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Site", Exact = true });

    public async Task<SiteDetailsPage> CreateSiteAsync(CreateSiteDto site)
    {
        await Name.FillAsync(site.Name);
        await Slug.FillAsync(site.Slug);
        await Status.SelectOptionAsync(site.Status);
        await Description.FillAsync(site.Description);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create", Exact = true }).ClickAsync();

        SiteDetailsPage detailsPage = GetPage<SiteDetailsPage>();
        await detailsPage.WaitUntilLoadedAsync();
        return detailsPage;
    }

    public async Task<SiteDetailsPage> UpdateAsync(string status, string description)
    {
        await Status.SelectOptionAsync(status);
        await Description.FillAsync(description);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

        SiteDetailsPage detailsPage = GetPage<SiteDetailsPage>();
        await detailsPage.WaitUntilLoadedAsync();
        return detailsPage;
    }
}
