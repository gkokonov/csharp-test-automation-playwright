using System.Text;
using Allure.Net.Commons;
using CsharpTestAutomation.Framework.Common.Utilities;
using Microsoft.Playwright;
using NLog;

namespace CsharpTestAutomation.Framework.Common.Reporting.Allure;

/// <summary>
/// UI-focused Allure helpers for attaching screenshots and browser console logs to reports.
/// API request/response logging is handled automatically by
/// <see cref="CsharpTestAutomation.Framework.API.Interceptors.ApiLoggingInterceptor"/>.
/// </summary>
public static class AllureExtensions
{
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Captures a screenshot of the current page and attaches it to the Allure report.
    /// </summary>
    /// <param name="page">Current page.</param>
    /// <param name="title">Title of the screenshot shown in the report.</param>
    public static async Task CaptureScreenshotAsync(IPage page, string title)
    {
        try
        {
            if (page == null)
            {
                s_log.Warn("Cannot capture screenshot - page is null");
                return;
            }

            var testId = TestIdentifier.GetTestId();
            var screenshotDir = GetAllureArtifactsDirectory();
            var fileName = $"{DateTimeOffset.UtcNow.Ticks}_{testId}.png";
            var filePath = Path.Combine(screenshotDir, fileName);

            await page.ScreenshotAsync(new PageScreenshotOptions {
                Path = filePath,
                FullPage = false
            });

            AllureApi.AddAttachment(title, "image/png", filePath);
        }
        catch (Exception ex)
        {
            s_log.Error(ex, "Failed to capture screenshot");
        }
    }

    /// <summary>
    /// Captures browser console logs for every open page in <paramref name="context"/> and
    /// attaches them to the Allure report.
    /// </summary>
    /// <param name="context">The browser context whose pages are inspected.</param>
    /// <param name="title">Attachment title; defaults to <c>"Browser Logs"</c>.</param>
    public static async Task CaptureBrowserLogsAsync(IBrowserContext context, string title = null)
    {
        try
        {
            if (context == null)
            {
                s_log.Warn("Cannot capture browser logs - context is null");
                return;
            }

            var logs = new List<string>
            {
                $"--- Browser Logs Captured at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---"
            };

            foreach (IPage page in context.Pages)
            {
                logs.Add($"=== Logs from page: {page.Url} ===");

                try
                {
                    var jsErrors = await page.EvaluateAsync<string>(@"
                            (function() {
                                if (window.jsErrors && window.jsErrors.length) {
                                    return window.jsErrors.join('\n');
                                }
                                return '';
                            })()
                        ");

                    if (!string.IsNullOrEmpty(jsErrors))
                    {
                        logs.Add("JavaScript Errors:");
                        logs.Add(jsErrors);
                    }
                }
                catch (Exception ex)
                {
                    logs.Add($"Could not retrieve JS errors: {ex.Message}");
                }
            }

            // Attach directly from memory — no temp-file round-trip needed.
            var contentBytes = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, logs));
            AllureApi.AddAttachment(title ?? "Browser Logs", "text/plain", contentBytes);
        }
        catch (Exception ex)
        {
            s_log.Error(ex, "Failed to capture browser logs");
        }
    }

    private static string GetAllureArtifactsDirectory()
    {
        var dir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "allure-results");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
