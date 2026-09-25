using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.UI;
using CsharpTestAutomation.Tests.Configurations;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI;

/// <summary>
/// Shared base for every UI object — both full-page objects and reusable components. Centralizes
/// access to the Playwright <see cref="IPage"/>, strongly-typed configuration, and web-first
/// assertions so the page and component layers stay thin and consistent.
/// </summary>
public abstract class BaseUIObject(IPage page)
{
    protected static readonly ExtendedConfiguration ExtendedConfiguration = AppConfiguration<ExtendedConfiguration>.Instance.Settings;

    static BaseUIObject()
    {
        Assertions.SetDefaultExpectTimeout(PlaywrightTimeouts.ExpectTimeoutInMS);
    }

    protected IPage Page => page;

    /// <summary>
    /// Web-first assertion entry point. Prefer these auto-retrying assertions over manual waits.
    /// </summary>
    public ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
