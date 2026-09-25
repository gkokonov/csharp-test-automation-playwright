using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Components;

/// <summary>
/// Base class for reusable UI components (fragments) such as navigation bars, data grids, dialogs,
/// and forms. A component is scoped to a <see cref="Root"/> locator so the same fragment can be
/// reused across pages. Favor composing components inside page objects over deriving pages from
/// them (composition over inheritance).
/// </summary>
public abstract class BaseUIComponent(IPage page, ILocator root) : BaseUIObject(page)
{
    /// <summary>
    /// Root locator that scopes this component. Build child locators from it
    /// (<c>Root.GetByRole(...)</c>, <c>Root.Locator(...)</c>) so the component stays isolated and
    /// can appear multiple times on a page without ambiguity.
    /// </summary>
    protected ILocator Root => root;
}
