using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CsharpTestAutomation.Framework.Common.Reporting.Allure;
using CsharpTestAutomation.Framework.UI;
using CsharpTestAutomation.Tests.Authentication;
using CsharpTestAutomation.Tests.UI.Pages;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.Tests;

/// <summary>
/// Base class for UI tests using Playwright.
/// </summary>
[Category("UI")]
public abstract class UiTestBase : TestBase
{
    private IBrowserContext? _context;
    private IPage? _page;

    [AllowNull]
    protected IBrowserContext Context {
        get => _context ?? throw new InvalidOperationException("Browser context is not initialized.");
        private set => _context = value;
    }

    [AllowNull]
    protected IPage Page {
        get => _page ?? throw new InvalidOperationException("Page is not initialized.");
        private set => _page = value;
    }

    /// <summary>
    /// Creates a page object bound to the current <see cref="Page"/>. Entry point for tests:
    /// <c>var login = GetPage&lt;MSLoginPage&gt;();</c>.
    /// </summary>
    protected T GetPage<T>() where T : BaseUIView => BaseUIView.Create<T>(Page);

    protected override async Task OnSetUpAsync() => await InitializePlaywrightEnvironmentAsync();

    protected override async Task OnTearDownAsync()
    {
        try
        {
            var testFailed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed;

            // Capture screenshot on failure
            if (testFailed && _page is { } page)
            {
                await CaptureScreenshotOnFailureAsync(page);

                // Also capture browser logs on failure if configured
                if (s_configuration.CaptureBrowserLogs && _context is { } context)
                {
                    await CaptureBrowserLogsOnFailureAsync(context);
                }
            }
        }
        finally
        {
            try { await PlaywrightBrowserFactory.DisposeContextAsync(); } catch { /* swallow */ }
            try { await PlaywrightBrowserFactory.DisposeBrowserAsync(); } catch { /* swallow */ }

            _context = null;
            _page = null;
        }
    }

    protected async Task InitializePlaywrightEnvironmentAsync(string? storageState = null)
    {
        await PlaywrightBrowserFactory.InitializeAsync();
        Context = await PlaywrightBrowserFactory.CreateContextAsync(storageState);
        Page = await PlaywrightBrowserFactory.CreatePageAsync();
    }

    protected async Task InitializeAuthenticatedPlaywrightEnvironmentAsync(BootstrapAuthState state)
    {
        await PlaywrightBrowserFactory.InitializeAsync();
        Context = await PlaywrightBrowserFactory.CreateContextAsync(state.StorageStateJson);
        await Context.AddInitScriptAsync(BuildSessionStorageRestoreScript(state.SessionStorageJson));
        Page = await PlaywrightBrowserFactory.CreatePageAsync();
    }

    // Playwright storage state does not capture sessionStorage, so restore it via an init script
    // that runs before any page script. The snapshot is JSON-encoded so embedded quotes are safe.
    private static string BuildSessionStorageRestoreScript(string sessionStorageJson)
    {
        var literal = JsonSerializer.Serialize(sessionStorageJson);
        return $$"""
            () => {
                try {
                    const entries = JSON.parse({{literal}});
                    for (const [key, value] of Object.entries(entries)) {
                        window.sessionStorage.setItem(key, value);
                    }
                } catch { }
            }
            """;
    }

    /// <summary>
    /// Captures a screenshot when a test fails and attaches it to the report
    /// </summary>
    private static async Task CaptureScreenshotOnFailureAsync(IPage page) =>
        await AllureExtensions.CaptureScreenshotAsync(
            page,
            TestContext.CurrentContext.Test.MethodName ?? "Unknown test");

    /// <summary>
    /// Captures browser logs when a test fails and attaches them to the report
    /// </summary>
    private static async Task CaptureBrowserLogsOnFailureAsync(IBrowserContext context) =>
        await AllureExtensions.CaptureBrowserLogsAsync(context, TestContext.CurrentContext.Test.MethodName);

}
