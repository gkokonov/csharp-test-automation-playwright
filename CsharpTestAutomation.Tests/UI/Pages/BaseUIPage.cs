using CsharpTestAutomation.Framework.UI;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Pages;

/// <summary>
/// Base class for full-page objects. A page owns the whole screen (navigation, page-level
/// readiness) and composes components for reusable fragments. Declare <see cref="PageReadyLocator"/>
/// to enable enforced readiness checks via <see cref="IsLoadedAsync"/> and
/// <see cref="WaitUntilLoadedAsync"/>, and use <see cref="GetPage{TPage}"/> to return the next page
/// object from an action that already navigated.
/// </summary>
public abstract class BaseUIPage(IPage page) : BaseUIObject(page)
{
    /// <summary>
    /// Locator that becomes visible only once this page has finished loading (for example a unique
    /// heading or primary control). Each page declares its readiness signal so readiness can be
    /// verified consistently.
    /// </summary>
    protected abstract ILocator PageReadyLocator { get; }

    /// <summary>
    /// Returns whether the page's readiness signal is currently visible, without waiting. Use for
    /// branching; use <see cref="WaitUntilLoadedAsync"/> to enforce readiness.
    /// </summary>
    public virtual Task<bool> IsLoadedAsync() => PageReadyLocator.IsVisibleAsync();

    /// <summary>
    /// Waits for the page's readiness signal and throws if it does not appear within
    /// <see cref="PlaywrightTimeouts.LongTimeoutInMS"/> (full page loads usually take longer than the
    /// default Expect timeout). Call after navigation to fail fast when the expected page did not load.
    /// </summary>
    public virtual async Task WaitUntilLoadedAsync() =>
        await Expect(PageReadyLocator).ToBeVisibleAsync(new() { Timeout = PlaywrightTimeouts.LongTimeoutInMS });

    /// <summary>
    /// Builds the page object the user lands on after an action that already performed the
    /// navigation, so callers can write <c>return GetPage&lt;NextPage&gt;();</c>.
    /// </summary>
    protected TPage GetPage<TPage>() where TPage : BaseUIPage => Create<TPage>(Page);

    public static TPage Create<TPage>(IPage page) where TPage : BaseUIPage =>
        (TPage)Activator.CreateInstance(typeof(TPage), page)!;
}
