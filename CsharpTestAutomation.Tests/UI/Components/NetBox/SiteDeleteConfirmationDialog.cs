using CsharpTestAutomation.Tests.UI.Components;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Components.NetBox;

public sealed class SiteDeleteConfirmationDialog(IPage page, ILocator root) : BaseUIComponent(page, root)
{
    public async Task ConfirmAsync()
    {
        await Expect(Root).ToBeVisibleAsync();
        await Root.GetByRole(AriaRole.Button, new() { Name = "Delete", Exact = true }).ClickAsync();
    }
}
