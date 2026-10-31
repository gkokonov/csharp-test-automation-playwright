using System.Diagnostics.CodeAnalysis;
using CsharpTestAutomation.Framework.Common.Reporting.Allure;
using CsharpTestAutomation.Framework.UI;
using CsharpTestAutomation.Tests.UI.Pages;
using Microsoft.Playwright;
using NLog;

namespace CsharpTestAutomation.Tests.Tests;

/// <summary>
/// Base class for UI tests using Playwright.
/// </summary>
[Category("UI")]
public abstract class UiTestBase : TestBase
{
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

    private IBrowserContext? _context;
    private IPage? _page;

    [AllowNull]
    protected IBrowserContext Context
    {
        get => _context ?? throw new InvalidOperationException("Browser context is not initialized.");
        private set => _context = value;
    }

    [AllowNull]
    protected IPage Page
    {
        get => _page ?? throw new InvalidOperationException("Page is not initialized.");
        private set => _page = value;
    }

    /// <summary>
    /// Creates a page object bound to the current <see cref="Page"/>. Entry point for tests:
    /// <c>var login = GetPage&lt;MSLoginPage&gt;();</c>.
    /// </summary>
    protected T GetPage<T>() where T : BaseUIPage => BaseUIPage.Create<T>(Page);

    protected override async Task OnSetUpAsync() => await InitializePlaywrightEnvironmentAsync();

    protected override async Task OnTearDownAsync()
    {
        bool testFailed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed;
        List<Exception> errors = [];

        if (testFailed && _page is { } page)
        {
            await AttemptTeardownAsync(() => CaptureScreenshotOnFailureAsync(page), errors);
            if (s_configuration.CaptureBrowserLogs && _context is { } context)
            {
                await AttemptTeardownAsync(() => CaptureBrowserLogsOnFailureAsync(context), errors);
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException("UI failure evidence capture failed.", errors);
        }
    }

    protected override async Task OnReleaseResourcesAsync(bool testFailed)
    {
        List<Exception> errors = [];
        await AttemptTeardownAsync(() => AllureExtensions.CaptureTraceAsync(testFailed), errors);
        try
        {
            await AttemptTeardownAsync(PlaywrightBrowserFactory.DisposeContextAsync, errors);
            await AttemptTeardownAsync(PlaywrightBrowserFactory.DisposeBrowserAsync, errors);
        }
        finally
        {
            _context = null;
            _page = null;
        }

        if (errors.Count > 0)
        {
            throw new AggregateException("UI test teardown failed.", errors);
        }
    }

    protected async Task InitializePlaywrightEnvironmentAsync(string? storageState = null)
    {
        await PlaywrightBrowserFactory.InitializeAsync();
        Context = await PlaywrightBrowserFactory.CreateContextAsync(storageState);
        Page = await PlaywrightBrowserFactory.CreatePageAsync();
    }

    /// <summary>
    /// Captures a screenshot when a test fails and attaches it to the report
    /// </summary>
    private static async Task CaptureScreenshotOnFailureAsync(IPage page) =>
        await AllureExtensions.CaptureScreenshotAsync(
            page, TestContext.CurrentContext.Test.MethodName ?? "Unknown test");

    /// <summary>
    /// Captures browser logs when a test fails and attaches them to the report
    /// </summary>
    private static async Task CaptureBrowserLogsOnFailureAsync(IBrowserContext context) =>
        await AllureExtensions.CaptureBrowserLogsAsync(context, TestContext.CurrentContext.Test.MethodName);

    private static async Task AttemptTeardownAsync(Func<Task> action, List<Exception> errors)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            errors.Add(ex);
            s_log.Error(ex, "UI test teardown action failed.");
            TestContext.Error.WriteLine($"Secondary UI teardown error: {ex}");
        }
    }

}
