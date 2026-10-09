using Microsoft.Playwright;

namespace CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

public sealed class DeviceDetailsPage(IPage page) : BaseUIPage(page)
{
    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true });

    public ILocator NameHeading(string name) => Page.GetByRole(AriaRole.Heading, new() { Name = name, Exact = true });

    public ILocator SiteLink(string name) => GetAttributeValue("Site").GetByRole(AriaRole.Link, new() { Name = name, Exact = true });

    public ILocator DeviceTypeLink(string model) => GetAttributeValue("Model").GetByRole(AriaRole.Link, new() { Name = model, Exact = true });

    public ILocator RoleLink(string name) => GetAttributeValue("Role").GetByRole(AriaRole.Link, new() { Name = name, Exact = true });

    public ILocator Status => GetAttributeValue("Status");

    public ILocator Description => GetAttributeValue("Description");

    private ILocator GetAttributeValue(string attribute) =>
        Page.GetByRole(AriaRole.Row).Filter(new() { Has = Page.GetByRole(AriaRole.Rowheader, new() { Name = attribute, Exact = true }) })
            .GetByRole(AriaRole.Cell).Last;
}
