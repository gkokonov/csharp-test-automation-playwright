using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.UI;
using CsharpTestAutomation.Tests.Configurations;
using CsharpTestAutomation.Tests.UI.Pages;
using CsharpTestAutomation.Tests.UI.Pages.NetBox;
using Microsoft.Playwright;
using NLog;

namespace CsharpTestAutomation.Tests.Tests.UI.NetBox;

[SetUpFixture]
public sealed class NetBoxUiSetupFixture
{
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
    private static readonly ExtendedConfiguration s_configuration = AppConfiguration<ExtendedConfiguration>.Instance.Settings;

    public static string StorageStatePath => Path.GetFullPath(Path.Combine(
        s_configuration.Ui.StorageStateDirectory,
        "netbox.json"));

    [OneTimeSetUp]
    public async Task CaptureStorageStateAsync()
    {
        if (string.IsNullOrWhiteSpace(s_configuration.NetBox.Username)
            || string.IsNullOrWhiteSpace(s_configuration.NetBox.Password))
        {
            throw new InvalidOperationException(
                "NetBox.Username and NetBox.Password must be configured for UI authentication.");
        }

        if (File.Exists(StorageStatePath))
        {
            File.Delete(StorageStatePath);
        }

        string storageStateDirectory = Path.GetDirectoryName(StorageStatePath)
            ?? throw new InvalidOperationException("Could not resolve the NetBox UI storage-state directory.");
        Directory.CreateDirectory(storageStateDirectory);

        await PlaywrightBrowserFactory.InitializeAsync();
        try
        {
            IBrowserContext context = await PlaywrightBrowserFactory.CreateContextAsync();
            IPage page = await PlaywrightBrowserFactory.CreatePageAsync();
            NetBoxLoginPage loginPage = BaseUIView.Create<NetBoxLoginPage>(page);
            await loginPage.OpenAsync();
            await loginPage.SignInAsync(s_configuration.NetBox.Username, s_configuration.NetBox.Password);
            await context.StorageStateAsync(new() { Path = StorageStatePath });

            s_log.Info($"Captured shared NetBox UI storage state at '{StorageStatePath}'.");
        }
        finally
        {
            await PlaywrightBrowserFactory.DisposeContextAsync();
            await PlaywrightBrowserFactory.DisposeBrowserAsync();
        }
    }

    [OneTimeTearDown]
    public void RemoveStorageState()
    {
        if (File.Exists(StorageStatePath))
        {
            File.Delete(StorageStatePath);
        }
    }
}
