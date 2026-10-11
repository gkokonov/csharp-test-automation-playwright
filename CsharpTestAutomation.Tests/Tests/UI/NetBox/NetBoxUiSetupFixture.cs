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

        Exception? primaryError = null;
        await PlaywrightBrowserFactory.InitializeAsync();
        try
        {
            IBrowserContext context = await PlaywrightBrowserFactory.CreateContextAsync();
            IPage page = await PlaywrightBrowserFactory.CreatePageAsync();
            NetBoxLoginPage loginPage = BaseUIPage.Create<NetBoxLoginPage>(page);
            await loginPage.OpenAsync();
            await loginPage.SignInAsync(s_configuration.NetBox.Username, s_configuration.NetBox.Password);
            await context.StorageStateAsync(new() { Path = StorageStatePath });

            s_log.Info($"Captured shared NetBox UI storage state at '{StorageStatePath}'.");
        }
        catch (Exception ex)
        {
            primaryError = ex;
            try { await PlaywrightBrowserFactory.StopTracingAsync(testFailed: true); }
            catch (Exception traceError) { s_log.Error(traceError, "Could not retain authentication trace."); }
            throw;
        }
        finally
        {
            List<Exception> errors = [];
            try { await PlaywrightBrowserFactory.DisposeContextAsync(); }
            catch (Exception ex) { errors.Add(ex); }
            try { await PlaywrightBrowserFactory.DisposeBrowserAsync(); }
            catch (Exception ex) { errors.Add(ex); }
            foreach (Exception error in errors)
            {
                s_log.Error(error, "Authentication browser release failed.");
            }

            if (primaryError is null && errors.Count > 0)
            {
                throw new AggregateException("Authentication browser release failed.", errors);
            }
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
