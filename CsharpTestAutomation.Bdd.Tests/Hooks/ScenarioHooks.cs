using System.Text;
using Allure.Net.Commons;
using CsharpTestAutomation.Bdd.Tests.Api.Clients;
using CsharpTestAutomation.Bdd.Tests.Authentication;
using CsharpTestAutomation.Bdd.Tests.Lifecycle;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.API.Clients;
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
    [BeforeScenario("Site", Order = 10)]
    public void SetMetadata()
    {
        string[] tags = scenario.ScenarioInfo.CombinedTags;
        (string story, SeverityLevel severity) = SiteScenarioMetadata.Get(tags);
        AllureLifecycle.Instance.UpdateTestCase(result => result.labels.RemoveAll(label => label.name == "feature"));
        AllureApi.AddSuite(tags.Contains("UI") ? "UI" : "API");
        AllureApi.AddEpic("NetBox");
        AllureApi.AddFeature("Site Management");
        AllureApi.AddStory(story);
        AllureApi.SetSeverity(severity);
        AllureApi.SetOwner(SiteScenarioMetadata.Owner);
    }

    [BeforeScenario("Site", Order = 20)]
    public async Task RegisterSiteDependenciesAsync()
    {
        BddRunResources resources = BddRunResources.Current;
        container.RegisterInstanceAs(resources.Configuration, dispose: false);
        var factory = new RestClientFactory(resources.Configuration.Api);
        var client = new SitesApiClient(factory, new NetBoxTokenAuthenticator(resources.TokenSession));
        lifecycle.AddRelease("Site API client", () =>
        {
            client.Dispose();
            return Task.CompletedTask;
        });
        container.RegisterInstanceAs(client, dispose: false);
        container.RegisterInstanceAs(await resources.GetSitesRepositoryAsync(), dispose: false);
    }

    [BeforeScenario("UI", Order = 30)]
    public async Task RegisterUiDependenciesAsync()
    {
        BddRunResources resources = BddRunResources.Current;
        string storageState = await resources.GetStorageStateAsync();
        await PlaywrightBrowserFactory.InitializeAsync();
        lifecycle.AddRelease("scenario browser", PlaywrightBrowserFactory.DisposeBrowserAsync);
        IBrowserContext context = await PlaywrightBrowserFactory.CreateContextAsync(storageState);
        lifecycle.AddRelease("scenario context", PlaywrightBrowserFactory.DisposeContextAsync);
        IPage page = await PlaywrightBrowserFactory.CreatePageAsync();
        container.RegisterInstanceAs(PlaywrightBrowserFactory.GetCurrentBrowser(), dispose: false);
        container.RegisterInstanceAs(context, dispose: false);
        container.RegisterInstanceAs(page, dispose: false);

        lifecycle.AddEvidence("screenshot", async () =>
        {
            byte[] bytes = await page.ScreenshotAsync();
            AllureApi.AddAttachment("Screenshot", "image/png", bytes);
        });
        if (resources.Configuration.CaptureBrowserLogs)
        {
            lifecycle.AddEvidence("browser logs", async () =>
            {
                string logs = await page.EvaluateAsync<string>("() => (window.jsErrors || []).join('\\n')");
                AllureApi.AddAttachment("Browser logs", "text/plain", Encoding.UTF8.GetBytes(logs));
            });
        }
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
