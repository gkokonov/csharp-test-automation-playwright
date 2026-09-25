using CsharpTestAutomation.Framework.Common;

namespace CsharpTestAutomation.Framework.UI;

/// <summary>
/// Timeout values in milliseconds for Playwright operations, sourced from
/// <see cref="CoreConfiguration"/> so they can be overridden per environment
/// via <c>appsettings.{env}.json</c> without recompilation.
/// <para>
/// Playwright internal defaults for reference:
/// browser launch = 180 000 ms, navigation = 30 000 ms, actions = 30 000 ms.
/// </para>
/// </summary>
public static class PlaywrightTimeouts
{
    private static CoreConfiguration Config => AppConfiguration<CoreConfiguration>.Instance.Settings;

    public static float BrowserStartTimeoutInMS => Config.BrowserStartTimeoutInMs;
    public static float NavigationTimeoutInMS => Config.NavigationTimeoutInMs;
    public static float ActionsTimeoutInMS => Config.ActionsTimeoutInMs;
    public static float ShortTimeoutInMS => Config.ShortTimeoutInMs;
    public static float MediumTimeoutInMS => Config.MediumTimeoutInMs;
    public static float LongTimeoutInMS => Config.LongTimeoutInMs;
    public static float ExpectTimeoutInMS => Config.ExpectTimeoutInMs;
}
