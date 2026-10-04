using System.Text.RegularExpressions;
using CsharpTestAutomation.Tests.Api.Dtos.Ipam;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Pages.NetBox;

public sealed class PrefixEditPage(IPage page) : BaseUIPage(page)
{
    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Add a new prefix", Exact = true });

    public async Task<PrefixDetailsPage> CreatePrefixAsync(CreatePrefixDto prefix)
    {
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Prefix" }).FillAsync(prefix.Prefix);
        await Page.GetByRole(AriaRole.Combobox, new() { Name = "Status" }).FillAsync(prefix.Status);
        await Page.GetByRole(AriaRole.Listbox, new() { Name = "Status" }).GetByRole(AriaRole.Option, new()
        {
            NameRegex = new Regex($"^{Regex.Escape(prefix.Status)}\\b", RegexOptions.IgnoreCase)
        }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description", Exact = true }).FillAsync(prefix.Description);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create", Exact = true }).ClickAsync();

        PrefixDetailsPage detailsPage = GetPage<PrefixDetailsPage>();
        await detailsPage.WaitUntilLoadedAsync();
        return detailsPage;
    }
}
