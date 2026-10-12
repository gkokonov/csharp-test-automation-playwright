using System.Text;
using Allure.Net.Commons;
using CsharpTestAutomation.Bdd.Tests.Lifecycle;
using CsharpTestAutomation.Framework.Common.Reporting.Allure;
using CsharpTestAutomation.Framework.UI;
using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;

namespace CsharpTestAutomation.Bdd.Tests.Hooks;

[Binding]
public sealed class ScenarioHooks(IObjectContainer container, ScenarioContext scenario,
    ScenarioLifecycle lifecycle,
    IReqnrollOutputHelper output)
{
    [BeforeScenario("NetBox", Order = 15)]
    public Task CheckDependenciesAsync() =>
        BddRunResources.Current.EnsureDependenciesAsync(scenario.ScenarioInfo.CombinedTags.Contains("UI"));

    [BeforeScenario("NetBox", Order = 20)]
    public void RegisterConfiguration()
    {
        container.RegisterInstanceAs(BddRunResources.Current.Configuration, dispose: false);
    }

    [BeforeScenario("UI", Order = 30)]
    public async Task RegisterUiDependenciesAsync()
    {
        BddRunResources resources = BddRunResources.Current;
        string storageState = await resources.GetStorageStateAsync();
        await PlaywrightBrowserFactory.InitializeAsync();
        lifecycle.AddRelease("scenario browser", PlaywrightBrowserFactory.DisposeBrowserAsync);
        lifecycle.AddRelease("scenario context", PlaywrightBrowserFactory.DisposeContextAsync);
        lifecycle.AddRelease("scenario trace", () => AllureExtensions.CaptureTraceAsync(scenario.TestError is not null));
        lifecycle.AddEvidence("Playwright trace", () => AllureExtensions.CaptureTraceAsync(testFailed: true));
        IBrowserContext context = await PlaywrightBrowserFactory.CreateContextAsync(storageState);
        if (resources.Configuration.CaptureBrowserLogs)
        {
            lifecycle.AddEvidence("browser logs", () => AllureExtensions.CaptureBrowserLogsAsync(context, "Browser logs"));
        }

        IPage page = await PlaywrightBrowserFactory.CreatePageAsync();
        container.RegisterInstanceAs(PlaywrightBrowserFactory.GetCurrentBrowser(), dispose: false);
        container.RegisterInstanceAs(context, dispose: false);
        container.RegisterInstanceAs(page, dispose: false);

        lifecycle.AddEvidence("screenshot", () => AllureExtensions.CaptureScreenshotAsync(page, "Screenshot"));
    }

    // Allure.Reqnroll 2.15.0 emits results on ScenarioFinishedEvent after AfterScenario hooks.
    [AfterScenario(Order = 100)]
    public async Task CompleteScenarioAsync()
    {
        AggregateException? error = await lifecycle.CompleteAsync(scenario.TestError, ReportSecondaryError);
        if (error is not null)
        {
            // Reqnroll records hook exceptions; AssertionException maps to failed in NUnit and Allure.
            throw new AssertionException(error.Message, error);
        }
    }

    [AfterTestRun]
    public static Task ReleaseRunResourcesAsync() => BddRunResources.ReleaseAsync((name, error) =>
        TestContext.Error.WriteLine($"Could not release {name}: {error}"));

    private void ReportSecondaryError(string name, Exception error)
    {
        string message = $"{name}: {error}";
        output.WriteLine(message);
        AllureApi.AddAttachment($"Secondary error: {name}", "text/plain", Encoding.UTF8.GetBytes(message));
    }

}
