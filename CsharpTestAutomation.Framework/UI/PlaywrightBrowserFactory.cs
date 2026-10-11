using System.Collections.Concurrent;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.Common.Utilities;
using Microsoft.Playwright;
using NLog;

namespace CsharpTestAutomation.Framework.UI;

/// <summary>
/// Factory for creating and managing Playwright browser instances with thread-safety for
/// parallel execution
/// </summary>
public static class PlaywrightBrowserFactory
{
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
    private static readonly ConcurrentDictionary<string, IBrowser> s_browsers = new();
    private static readonly ConcurrentDictionary<string, IBrowserContext> s_contexts = new();
    private static readonly ConcurrentDictionary<IBrowserContext, BrowserContextDiagnostics> s_diagnostics = new();
    private static readonly SemaphoreSlim s_playwrightLock = new(1, 1);
    private static volatile IPlaywright? s_playwrightInstance;

    private static readonly CoreConfiguration s_configuration = AppConfiguration<CoreConfiguration>.Instance.Settings;

    /// <summary>
    /// Gets the current browser for the test
    /// </summary>
    public static IBrowser GetCurrentBrowser()
    {
        string testId = GetCurrentTestId();
        return s_browsers.TryGetValue(testId, out IBrowser? browser)
            ? browser
            : throw new InvalidOperationException($"Browser not initialized for test {testId}");
    }

    /// <summary>
    /// Gets the current browser context for the test
    /// </summary>
    public static IBrowserContext GetCurrentContext()
    {
        string testId = GetCurrentTestId();
        return s_contexts.TryGetValue(testId, out IBrowserContext? context)
            ? context
            : throw new InvalidOperationException($"Browser context not initialized for test {testId}");
    }

    /// <summary>
    /// Initializes Playwright and creates browser instance for current test
    /// </summary>
    public static async Task InitializeAsync()
    {
        string testId = GetCurrentTestId();

        // Initialize Playwright with thread-safe locking
        if (s_playwrightInstance == null)
        {
            await s_playwrightLock.WaitAsync().ConfigureAwait(false);
            try
            {
                s_playwrightInstance ??= await Playwright.CreateAsync().ConfigureAwait(false);
            }
            finally
            {
                s_playwrightLock.Release();
            }
        }

        // Create browser instance for this test if not exists
        if (!s_browsers.ContainsKey(testId))
        {
            BrowserType browserType = Enum.Parse<BrowserType>(s_configuration.BrowserType.ToUpper());
            IBrowser browser = await CreateBrowserInstanceAsync(s_playwrightInstance, browserType).ConfigureAwait(false);
            s_browsers[testId] = browser;
        }
    }

    /// <summary>
    /// Creates a new browser context for the current test. <see langword="null"/> for <paramref
    /// name="storageState"/> means no storage state is used.
    /// </summary>
    public static Task<IBrowserContext> CreateContextAsync(string? storageState = null) => CreateAndStoreContextAsync(storageState, s_configuration.HttpCredentials);

    /// <summary>
    /// Creates a new browser context for the current test with given HTTP credentials.
    /// <see langword="null"/> for <paramref name="storageState"/> means no storage state is used.
    /// </summary>
    public static Task<IBrowserContext> CreateContextWithGivenHttpCredentialsAsync(string userName, string password, string? storageState = null)
    {
        var credentials = new HttpCredentials { Username = userName, Password = password };
        return CreateAndStoreContextAsync(storageState, credentials);
    }

    /// <summary>
    /// Creates a new page in the current context
    /// </summary>
    public static async Task<IPage> CreatePageAsync()
    {
        string testId = GetCurrentTestId();
        try
        {
            IBrowserContext context = GetCurrentContext();
            IPage page = await context.NewPageAsync().ConfigureAwait(false);
            return page;
        }
        catch (Exception ex)
        {
            s_log.Error(ex, $"Failed to create new page for test {testId}");
            throw;
        }
    }

    /// <summary>Stops tracing and closes the context, reporting failures after all attempts.
    /// Call StopTracingAsync first to retain failure-only traces.</summary>
    public static async Task DisposeContextAsync()
    {
        string testId = GetCurrentTestId();
        if (s_contexts.TryRemove(testId, out IBrowserContext? context))
        {
            await CloseContextAsync(context, testId).ConfigureAwait(false);
        }
    }

    /// <summary>Stops the current context's trace and returns its saved path, or null when no trace is retained.</summary>
    /// <param name="testFailed">Whether the test failed; controls failure-only retention.</param>
    public static Task<string?> StopTracingAsync(bool testFailed)
    {
        string testId = GetCurrentTestId();
        return s_contexts.TryGetValue(testId, out IBrowserContext? context)
            && s_diagnostics.TryGetValue(context, out BrowserContextDiagnostics? diagnostics)
            ? diagnostics.StopTracingAsync(testFailed, testId)
            : Task.FromResult<string?>(null);
    }

    /// <summary>Returns console messages and page errors, including popups and pages already closed.</summary>
    /// <param name="context">A context created by this factory with CaptureBrowserLogs enabled.</param>
    public static IReadOnlyList<string> GetBrowserLogs(IBrowserContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return s_diagnostics.TryGetValue(context, out BrowserContextDiagnostics? diagnostics)
            ? diagnostics.GetLogs()
            : throw new InvalidOperationException("Browser diagnostics are not registered for this context.");
    }

    /// <summary>
    /// Disposes browser for current test
    /// </summary>
    public static async Task DisposeBrowserAsync()
    {
        string testId = GetCurrentTestId();

        if (s_browsers.TryRemove(testId, out IBrowser? browser))
        {
            try
            {
                await browser.CloseAsync().ConfigureAwait(false);
                s_log.Debug($"[{testId}] : Browser closed.");
            }
            catch (Exception ex)
            {
                s_log.Error(ex, $"Error closing browser for test {testId}");
                throw;
            }
        }
    }

    /// <summary>
    /// Disposes every browser, context, and the shared Playwright instance.
    /// This is process-wide. Call it only from an assembly-level teardown, or from a
    /// fixture that is <c>[NonParallelizable]</c> and owns the Playwright process.
    /// A parallel fixture must not call this method.
    /// </summary>
    public static async Task DisposeAllAsync()
    {
        List<Exception> errors = [];
        foreach (string testId in s_contexts.Keys.ToArray())
        {
            if (s_contexts.TryRemove(testId, out IBrowserContext? context))
            {
                try { await CloseContextAsync(context, testId).ConfigureAwait(false); }
                catch (Exception ex) { errors.Add(ex); }
            }
        }

        foreach (string testId in s_browsers.Keys.ToArray())
        {
            if (s_browsers.TryRemove(testId, out IBrowser? browser))
            {
                try { await browser.CloseAsync().ConfigureAwait(false); }
                catch (Exception ex) { errors.Add(ex); }
            }
        }

        try { s_playwrightInstance?.Dispose(); }
        catch (Exception ex) { errors.Add(ex); }
        finally { s_playwrightInstance = null; }

        if (errors.Count > 0)
        {
            throw new AggregateException("Playwright resource release failed.", errors);
        }
    }

    private static async Task CloseContextAsync(IBrowserContext context, string testId)
    {
        if (s_diagnostics.TryRemove(context, out BrowserContextDiagnostics? diagnostics))
        {
            await diagnostics.CloseAsync(testId).ConfigureAwait(false);
        }
        else
        {
            await context.CloseAsync().ConfigureAwait(false);
        }
    }

    private static async Task<IBrowserContext> CreateAndStoreContextAsync(string? storageState, HttpCredentials? httpCredentials)
    {
        string testId = GetCurrentTestId();

        if (!s_browsers.TryGetValue(testId, out IBrowser? browser))
        {
            throw new InvalidOperationException($"Browser must be initialized before creating context for test {testId}");
        }

        // Dispose any stale context from a previous call (e.g. a test retry that skipped teardown)
        if (s_contexts.TryRemove(testId, out IBrowserContext? stale))
        {
            await CloseContextAsync(stale, testId).ConfigureAwait(false);
        }

        IBrowserContext context = await CreateBrowserContextAsync(browser, storageState, httpCredentials).ConfigureAwait(false);
        s_contexts[testId] = context;
        if (s_configuration.CaptureBrowserLogs || s_configuration.TraceEnabled || s_configuration.TraceOnFailure)
        {
            var diagnostics = new BrowserContextDiagnostics(context, s_configuration);
            s_diagnostics[context] = diagnostics;
            await diagnostics.StartTracingAsync().ConfigureAwait(false);
        }

        return context;
    }

    private static async Task<IBrowser> CreateBrowserInstanceAsync(IPlaywright playwright, BrowserType browserType)
    {
        BrowserTypeLaunchOptions noChannelOptions = CreateBrowserOptions();
        return browserType switch
        {
            BrowserType.CHROMIUM => await playwright.Chromium.LaunchAsync(noChannelOptions).ConfigureAwait(false),
            BrowserType.CHROME => await playwright.Chromium.LaunchAsync(CreateBrowserOptions("chrome")).ConfigureAwait(false),
            BrowserType.MSEDGE => await playwright.Chromium.LaunchAsync(CreateBrowserOptions("msedge")).ConfigureAwait(false),
            BrowserType.FIREFOX => await playwright.Firefox.LaunchAsync(noChannelOptions).ConfigureAwait(false),
            BrowserType.SAFARI => await playwright.Webkit.LaunchAsync(noChannelOptions).ConfigureAwait(false),
            _ => throw new ApplicationException("Unsupported BrowserType was provided: " + browserType),
        };
    }

    private static BrowserTypeLaunchOptions CreateBrowserOptions(string? channel = null)
    {
        var browserOptions = new BrowserTypeLaunchOptions
        {
            DownloadsPath = Directory.GetCurrentDirectory(),
            Timeout = PlaywrightTimeouts.BrowserStartTimeoutInMS,
            Headless = s_configuration.HeadlessMode,
            Proxy = s_configuration.ProxyMode ? new Proxy { Server = s_configuration.ProxyServer } : null,
            Channel = channel,
            SlowMo = s_configuration.PlaywrightSlowMotion,
            Args = s_configuration.PlaywrightArgs?.Split(' ')
        };

        return browserOptions;
    }

    private static async Task<IBrowserContext> CreateBrowserContextAsync(IBrowser browser, string? storageState, HttpCredentials? httpCredentials = null)
    {
        BrowserNewContextOptions contextOptions = CreateContextOptions(
            s_configuration,
            ResolveDeviceOptions(s_configuration.PlaywrightDeviceName),
            storageState,
            httpCredentials);

        IBrowserContext context = await browser.NewContextAsync(contextOptions).ConfigureAwait(false);

        context.SetDefaultNavigationTimeout(PlaywrightTimeouts.NavigationTimeoutInMS);
        context.SetDefaultTimeout(PlaywrightTimeouts.ActionsTimeoutInMS);

        return context;
    }

    /// <summary>
    /// Builds context options for one test. Device fields are copied onto a new object so the
    /// cached Playwright device descriptor is not mutated. Storage state, HTTP credentials, CSP
    /// bypass, and video settings still apply when a device name is set.
    /// </summary>
    internal static BrowserNewContextOptions CreateContextOptions(
        CoreConfiguration configuration,
        BrowserNewContextOptions? deviceOptions,
        string? storageState,
        HttpCredentials? httpCredentials)
    {
        // Storage state is either inline JSON (an object) or a file path; anything else is a path.
        bool isFilePath = !string.IsNullOrWhiteSpace(storageState) && !storageState.TrimStart().StartsWith('{');

        if (isFilePath && !File.Exists(storageState))
        {
            throw new FileNotFoundException($"Playwright storage state file was not found: '{storageState}'.", storageState);
        }

        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = deviceOptions?.ViewportSize ?? configuration.ViewportSize,
            UserAgent = deviceOptions?.UserAgent,
            DeviceScaleFactor = deviceOptions?.DeviceScaleFactor,
            IsMobile = deviceOptions?.IsMobile,
            HasTouch = deviceOptions?.HasTouch,
            ScreenSize = deviceOptions?.ScreenSize,
            IgnoreHTTPSErrors = true,
            RecordVideoDir = configuration.RecordVideoEnabled ? configuration.RecordDir : null,
            BypassCSP = configuration.BypassCSP,
            StorageStatePath = isFilePath ? storageState : null,
            StorageState = !isFilePath ? storageState : null,
            HttpCredentials = httpCredentials
        };

        return contextOptions;
    }

    private static BrowserNewContextOptions? ResolveDeviceOptions(string? deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            return null;
        }

        if (s_playwrightInstance is null || !s_playwrightInstance.Devices.TryGetValue(deviceName, out BrowserNewContextOptions? deviceOptions))
        {
            throw new InvalidOperationException($"Playwright device '{deviceName}' is not defined.");
        }

        return deviceOptions;
    }

    /// <summary>
    /// Gets the ID of the current test using TestIdentifier
    /// </summary>
    private static string GetCurrentTestId() => TestIdentifier.GetTestId();
}
