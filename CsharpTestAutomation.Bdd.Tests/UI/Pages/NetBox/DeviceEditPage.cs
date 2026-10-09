using System.Text.RegularExpressions;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Devices;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

public sealed class DeviceEditPage(IPage page) : BaseUIPage(page)
{
    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Add a new device", Exact = true });

    public async Task<DeviceDetailsPage> CreateDeviceAsync(CreateDeviceDto device, string site, string deviceType, string role)
    {
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Name", Exact = true }).FillAsync(device.Name);
        await SelectAsync("Device role", role);
        await SelectAsync("Device type", deviceType);
        await SelectAsync("Site", site);
        await SelectAsync("Status", device.Status);
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description", Exact = true }).FillAsync(device.Description);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create", Exact = true }).ClickAsync();

        DeviceDetailsPage detailsPage = GetPage<DeviceDetailsPage>();
        await detailsPage.WaitUntilLoadedAsync();
        return detailsPage;
    }

    private async Task SelectAsync(string label, string value)
    {
        ILocator input = Page.GetByRole(AriaRole.Combobox, new() { Name = label });
        await input.FillAsync(value);
        await Page.GetByRole(AriaRole.Option, new() { NameRegex = new Regex(Regex.Escape(value), RegexOptions.IgnoreCase) }).ClickAsync();
    }
}
