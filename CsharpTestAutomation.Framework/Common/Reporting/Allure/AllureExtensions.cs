using System.Text;
using Allure.Net.Commons;
using CsharpTestAutomation.Framework.UI;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Framework.Common.Reporting.Allure;

/// <summary>Shared UI evidence capture. Callers must attempt artifacts independently and report capture errors.</summary>
public static class AllureExtensions
{
    /// <summary>Attaches a screenshot from memory. Capture or attachment errors are reported to the caller.</summary>
    public static async Task CaptureScreenshotAsync(IPage? page, string title)
    {
        if (page is null)
        {
            return;
        }

        byte[] bytes = await page.ScreenshotAsync(new() { FullPage = false }).ConfigureAwait(false);
        AllureApi.AddAttachment(title, "image/png", bytes);
    }

    /// <summary>Attaches console messages and JavaScript errors collected from a factory-created context.</summary>
    public static Task CaptureBrowserLogsAsync(IBrowserContext? context, string? title = null)
    {
        if (context is not null)
        {
            IReadOnlyList<string> logs = PlaywrightBrowserFactory.GetBrowserLogs(context);
            byte[] bytes = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, logs));
            AllureApi.AddAttachment(title ?? "Browser logs", "text/plain", bytes);
        }

        return Task.CompletedTask;
    }

    /// <summary>Stops tracing and attaches the retained trace. Call before releasing the current context.</summary>
    public static async Task CaptureTraceAsync(bool testFailed)
    {
        string? path = await PlaywrightBrowserFactory.StopTracingAsync(testFailed).ConfigureAwait(false);
        if (path is not null)
        {
            AllureApi.AddAttachment("Playwright trace", "application/zip", path);
        }
    }
}
