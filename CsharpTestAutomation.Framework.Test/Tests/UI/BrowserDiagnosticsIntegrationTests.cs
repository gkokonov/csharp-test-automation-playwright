using Allure.NUnit;
using AwesomeAssertions;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.Common.Reporting.Allure;
using CsharpTestAutomation.Framework.UI;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Framework.Test.Tests.UI;

[TestFixture]
[NonParallelizable]
[AllureNUnit]
public class BrowserDiagnosticsIntegrationTests
{
    [TestCase(false, false, false)]
    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(false, false, true)]
    public async Task Verify_ContextLifecycle_When_DiagnosticsAreDisabledOrIndividuallyEnabled(
        bool logsEnabled, bool traceEnabled, bool traceOnFailure)
    {
        CoreConfiguration configuration = AppConfiguration<CoreConfiguration>.Instance.Settings;
        bool originalLogs = configuration.CaptureBrowserLogs;
        bool originalTrace = configuration.TraceEnabled;
        bool originalFailureTrace = configuration.TraceOnFailure;
        string originalDirectory = configuration.TraceDir;
        string directory = Path.Combine(Path.GetTempPath(), $"optional-diagnostics-{Guid.NewGuid():N}");
        configuration.CaptureBrowserLogs = logsEnabled;
        configuration.TraceEnabled = traceEnabled;
        configuration.TraceOnFailure = traceOnFailure;
        configuration.TraceDir = directory;
        try
        {
            await PlaywrightBrowserFactory.InitializeAsync();
            IBrowserContext context = await PlaywrightBrowserFactory.CreateContextAsync();
            IPage page = await PlaywrightBrowserFactory.CreatePageAsync();
            await page.EvaluateAsync("() => console.error('optional console')");
            if (logsEnabled || traceEnabled || traceOnFailure)
            {
                IReadOnlyList<string> logs = PlaywrightBrowserFactory.GetBrowserLogs(context);
                if (logsEnabled)
                {
                    logs.Should().Contain(x => x.Contains("optional console"));
                }
                else
                {
                    logs.Should().BeEmpty();
                }
            }
            else
            {
                Action getLogs = () => PlaywrightBrowserFactory.GetBrowserLogs(context);
                getLogs.Should().Throw<InvalidOperationException>().WithMessage("*not registered*");
            }

            string? tracePath = await PlaywrightBrowserFactory.StopTracingAsync(testFailed: true);
            (tracePath is not null).Should().Be(traceEnabled || traceOnFailure);
            await PlaywrightBrowserFactory.DisposeContextAsync();
            page.IsClosed.Should().BeTrue();
            PlaywrightBrowserFactory.GetCurrentBrowser().IsConnected.Should().BeTrue();
        }
        finally
        {
            try { await PlaywrightBrowserFactory.DisposeAllAsync(); }
            finally
            {
                configuration.CaptureBrowserLogs = originalLogs;
                configuration.TraceEnabled = originalTrace;
                configuration.TraceOnFailure = originalFailureTrace;
                configuration.TraceDir = originalDirectory;
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Verify_LogsAndTraceRetention_When_LocalPagesEmitErrors(bool failed)
    {
        CoreConfiguration configuration = AppConfiguration<CoreConfiguration>.Instance.Settings;
        bool originalLogs = configuration.CaptureBrowserLogs;
        bool originalTrace = configuration.TraceEnabled;
        bool originalFailureTrace = configuration.TraceOnFailure;
        string originalDirectory = configuration.TraceDir;
        string directory = Path.Combine(Path.GetTempPath(), $"browser-diagnostics-{Guid.NewGuid():N}");
        configuration.CaptureBrowserLogs = true;
        configuration.TraceEnabled = false;
        configuration.TraceOnFailure = true;
        configuration.TraceDir = directory;
        try
        {
            await PlaywrightBrowserFactory.InitializeAsync();
            IBrowserContext context = await PlaywrightBrowserFactory.CreateContextAsync();
            IPage page = await PlaywrightBrowserFactory.CreatePageAsync();
            await page.SetContentAsync("<script>console.error('main console'); throw new Error('main error');</script>");
            Task<IPage> popupTask = context.WaitForPageAsync();
            await page.EvaluateAsync("() => window.open('about:blank')");
            IPage popup = await popupTask;
            await popup.SetContentAsync("<script>console.warn('popup console'); throw new Error('popup error');</script>");
            await popup.CloseAsync();

            IReadOnlyList<string> logs = PlaywrightBrowserFactory.GetBrowserLogs(context);
            logs.Should().Contain(x => x.Contains("main console"));
            logs.Should().Contain(x => x.Contains("main error"));
            logs.Should().Contain(x => x.Contains("popup console"));
            logs.Should().Contain(x => x.Contains("popup error"));
            await AllureExtensions.CaptureScreenshotAsync(page, "Local screenshot");
            await AllureExtensions.CaptureBrowserLogsAsync(context);
            await AllureExtensions.CaptureTraceAsync(failed);
            string[] traces = Directory.Exists(directory) ? Directory.GetFiles(directory, "*.zip") : [];
            traces.Should().HaveCount(failed ? 1 : 0);
            if (failed)
            {
                new FileInfo(traces[0]).Length.Should().BePositive();
            }
        }
        finally
        {
            try { await PlaywrightBrowserFactory.DisposeAllAsync(); }
            finally
            {
                configuration.CaptureBrowserLogs = originalLogs;
                configuration.TraceEnabled = originalTrace;
                configuration.TraceOnFailure = originalFailureTrace;
                configuration.TraceDir = originalDirectory;
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }
    }
}
