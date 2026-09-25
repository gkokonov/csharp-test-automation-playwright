using CsharpTestAutomation.Framework.API.Configuration;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Framework.Common;

/// <summary>
/// Base configuration class for Playwright-based test automation.
/// Provides settings for browser selection, headless mode, proxy, tracing, video recording,
/// reporting output paths, viewport size, slow motion, device emulation, HTTP credentials,
/// and additional Playwright launch arguments.
/// Intended to be extended by project-specific configuration classes (e.g., <c>ExtendedConfiguration</c>).
/// Values are typically loaded from <c>appsettings.json</c> via <c>AppConfiguration&lt;T&gt;</c>.
/// </summary>
public class CoreConfiguration
{
    private static readonly string s_defaultOutputPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName ??
                           Environment.CurrentDirectory;

    public string BrowserType { get; set; } = "CHROMIUM"; // default if not set
    public bool HeadlessMode { get; set; } = true; // default if not set
    public string ProxyServer { get; set; } = "http://localhost:8090"; // default if not set
    public bool ProxyMode { get; set; }  // default false if not set
    public bool CaptureBrowserLogs { get; set; }  // default false if not set
    public bool TraceEnabled { get; set; }  // default false if not set
    public string TraceDir { get; set; } = Path.Combine(s_defaultOutputPath, "Traces");
    public string ReportDir { get; set; } = Path.Combine(s_defaultOutputPath, "Report");
    public bool RecordVideoEnabled { get; set; } // default false if not set
    public string RecordDir { get; set; } = Path.Combine(s_defaultOutputPath, "Videos");
    public ViewportSize ViewportSize { get; set; } = new ViewportSize { Width = 1280, Height = 720 }; // default if not set
    public float PlaywrightSlowMotion { get; set; } // default if not set, no slow motion
    public string PlaywrightDeviceName { get; set; } = string.Empty; // default if not set
    public string BuildNumber { get; set; } = "1.0.0"; // default if not set
    public bool BypassCSP { get; set; } // default false if not set
    public HttpCredentials HttpCredentials { get; set; }  // default null if not set
    public string PlaywrightArgs { get; set; } // e.g. "--disable-gpu --no-sandbox"

    public float BrowserStartTimeoutInMs { get; set; } = 35_000;
    public float NavigationTimeoutInMs { get; set; } = 35_000;
    public float ActionsTimeoutInMs { get; set; } = 10_000;
    public float ShortTimeoutInMs { get; set; } = 3_000;
    public float MediumTimeoutInMs { get; set; } = 6_000;
    public float LongTimeoutInMs { get; set; } = 12_000;
    public float ExpectTimeoutInMs { get; set; } = 6_000;

    public int DbQueryTimeoutSeconds { get; set; } = 180;
    public int DbExecuteTimeoutSeconds { get; set; } = 600;

    /// <summary>API testing settings. Configured via the <c>Api</c> key in <c>appsettings.json</c>.</summary>
    public ApiSettings Api { get; set; } = new();
}
