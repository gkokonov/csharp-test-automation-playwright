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
    private static readonly SemaphoreSlim s_playwrightLock = new(1, 1);
    private static volatile IPlaywright? s_playwrightInstance;

    private static readonly CoreConfiguration s_configuration = AppConfiguration<CoreConfiguration>.Instance.Settings;

    /// <summary>
    /// Gets the current browser for the test
    /// </summary>
    public static IBrowser GetCurrentBrowser()
    {
        var testId = GetCurrentTestId();
        return s_browsers.TryGetValue(testId, out IBrowser? browser)
            ? browser
            : throw new InvalidOperationException($"Browser not initialized for test {testId}");
    }

    /// <summary>
    /// Gets the current browser context for the test
    /// </summary>
    public static IBrowserContext GetCurrentContext()
    {
        var testId = GetCurrentTestId();
        return s_contexts.TryGetValue(testId, out IBrowserContext? context)
            ? context
            : throw new InvalidOperationException($"Browser context not initialized for test {testId}");
    }

    /// <summary>
    /// Initializes Playwright and creates browser instance for current test
    /// </summary>
    public static async Task InitializeAsync()
    {
        var testId = GetCurrentTestId();

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
        var testId = GetCurrentTestId();
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

    /// <summary>
    /// Disposes the current context, saving its trace to <see cref="CoreConfiguration.TraceDir"/>
    /// when <see cref="CoreConfiguration.TraceEnabled"/> is <see langword="true"/>.
    /// </summary>
    public static async Task DisposeContextAsync()
    {
        var testId = GetCurrentTestId();

        if (s_contexts.TryRemove(testId, out IBrowserContext? context))
        {
            if (s_configuration.TraceEnabled)
            {
                try
                {
                    var traceDir = s_configuration.TraceDir;
                    if (!Directory.Exists(traceDir))
                    {
                        Directory.CreateDirectory(traceDir);
                    }

                    var sanitizedName = SanitizeFileName(testId);
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                    var traceFile = Path.Combine(traceDir, $"{sanitizedName}_{timestamp}.zip");

                    await context.Tracing.StopAsync(new() { Path = traceFile }).ConfigureAwait(false);

                    s_log.Debug($"Saved Playwright trace to {traceFile}.");
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, $"Failed to stop Playwright tracing for test {testId}.");
                }
            }

            try
            {
                await context.CloseAsync().ConfigureAwait(false);
                s_log.Debug($"[{testId}] : Browser context closed.");
            }
            catch (Exception ex)
            {
                s_log.Error(ex, $"Error closing browser context for test {testId}");
            }
        }
    }

    /// <summary>
    /// Disposes browser for current test
    /// </summary>
    public static async Task DisposeBrowserAsync()
    {
        var testId = GetCurrentTestId();

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
        // Snapshot before iterating so concurrent additions/removals in parallel runs
        // cannot cause entries to be silently skipped or double-closed.
        foreach (KeyValuePair<string, IBrowserContext> pair in s_contexts.ToList())
        {
            try
            {
                await pair.Value.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, $"Error closing browser context for test {pair.Key}");
            }
        }

        s_contexts.Clear();

        foreach (KeyValuePair<string, IBrowser> pair in s_browsers.ToList())
        {
            try
            {
                await pair.Value.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, $"Error closing browser for test {pair.Key}");
            }
        }

        s_browsers.Clear();

        s_playwrightInstance?.Dispose();
        s_playwrightInstance = null;

        s_log.Debug("All Playwright resources disposed.");
    }

    private static async Task<IBrowserContext> CreateAndStoreContextAsync(string? storageState, HttpCredentials? httpCredentials)
    {
        var testId = GetCurrentTestId();

        if (!s_browsers.TryGetValue(testId, out IBrowser? browser))
        {
            throw new InvalidOperationException($"Browser must be initialized before creating context for test {testId}");
        }

        // Dispose any stale context from a previous call (e.g. a test retry that skipped teardown)
        if (s_contexts.TryRemove(testId, out IBrowserContext? stale))
        {
            try
            { await stale.CloseAsync().ConfigureAwait(false); }
            catch (Exception ex) { s_log.Warn(ex, $"Failed to close stale context for test {testId} before replacement"); }
        }

        IBrowserContext context = await CreateBrowserContextAsync(browser, storageState, httpCredentials).ConfigureAwait(false);
        s_contexts[testId] = context;

        return context;
    }

    private static async Task<IBrowser> CreateBrowserInstanceAsync(IPlaywright playwright, BrowserType browserType)
    {
        BrowserTypeLaunchOptions noChannelOptions = CreateBrowserOptions();
        return browserType switch {
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
        var browserOptions = new BrowserTypeLaunchOptions {
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

        if (s_configuration.TraceEnabled)
        {
            try
            {
                await context.Tracing.StartAsync(new() {
                    Screenshots = true,
                    Snapshots = true,
                    Sources = true
                });

                s_log.Debug("Started Playwright trace recording.");
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Failed to start tracing");
            }
        }

        return context;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Trim();
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
        var contextOptions = new BrowserNewContextOptions {
            ViewportSize = deviceOptions?.ViewportSize ?? configuration.ViewportSize,
            UserAgent = deviceOptions?.UserAgent,
            DeviceScaleFactor = deviceOptions?.DeviceScaleFactor,
            IsMobile = deviceOptions?.IsMobile,
            HasTouch = deviceOptions?.HasTouch,
            ScreenSize = deviceOptions?.ScreenSize,
            IgnoreHTTPSErrors = true,
            RecordVideoDir = configuration.RecordVideoEnabled ? configuration.RecordDir : null,
            BypassCSP = configuration.BypassCSP,
            StorageState = storageState,
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
