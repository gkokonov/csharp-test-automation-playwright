using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

public sealed class SiteEditPage(IPage page) : BaseUIPage(page)
{
    private ILocator Name => Page.GetByLabel("Name");

    private ILocator Slug => Page.GetByRole(AriaRole.Textbox, new() { Name = "Slug" });

    // NetBox's accessible combobox is a hidden helper input; the form value is held by this select.
    private ILocator Status => Page.Locator("select[name='status']");

    private ILocator Description => Page.GetByLabel("Description");

    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Site", Exact = true });

    public async Task<SiteDetailsPage> CreateSiteAsync(CreateSiteDto site)
    {
        await Name.FillAsync(site.Name);

        // Triple-click then type to simulate user keyboard input, so NetBox's JS marks the
        // slug field as manually edited and stops auto-generating it from the Name field.
        await Slug.ClickAsync(new() { ClickCount = 3 });
        await Slug.PressSequentiallyAsync(site.Slug, new() { Delay = 20 });

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
