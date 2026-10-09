using Microsoft.Playwright;

namespace CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;

public sealed class DevicesListPage(IPage page) : BaseUIPage(page)
{
    protected override ILocator PageReadyLocator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Devices", Exact = true });

    public async Task NavigateAsync()
    {
        Uri uri = new(new Uri(ExtendedConfiguration.NetBox.BaseUrl, UriKind.Absolute), "dcim/devices/");
        await Page.GotoAsync(uri.ToString());
        await WaitUntilLoadedAsync();
    }

    public async Task<DeviceEditPage> AddDeviceAsync()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).ClickAsync();
        DeviceEditPage editPage = GetPage<DeviceEditPage>();
        await editPage.WaitUntilLoadedAsync();
        return editPage;
    }
}
