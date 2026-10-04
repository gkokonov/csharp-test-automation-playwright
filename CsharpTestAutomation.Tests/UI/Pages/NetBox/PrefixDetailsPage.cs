using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Pages.NetBox;

public sealed class PrefixDetailsPage(IPage page) : BaseUIPage(page)
{
    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true });

    public ILocator PrefixHeading(string prefix) => Page.GetByRole(AriaRole.Heading, new() { Name = prefix, Exact = true });

    public ILocator Status => GetAttributeValue("Status");

    public ILocator Description => GetAttributeValue("Description");

    private ILocator GetAttributeValue(string attribute) =>
        Page.GetByRole(AriaRole.Row).Filter(new() { Has = Page.GetByRole(AriaRole.Rowheader, new() { Name = attribute, Exact = true }) })
            .GetByRole(AriaRole.Cell).Last;
}
