using Microsoft.Playwright;

namespace CsharpTestAutomation.Framework.Common.Extensions;

/// <summary>
/// Playwright ILocator extensions class.
/// </summary>
public static class LocatorExtensions
{
    private static readonly NLog.Logger s_log = NLog.LogManager.GetCurrentClassLogger();

    public static async Task<bool> SelectDropdownValueByRandomIndexAsync(this ILocator locator)
    {
        IReadOnlyList<ILocator> options = await locator.Locator("option").AllAsync();

        if (options.Count <= 1)
            return false;

        var randomIndex = Random.Shared.Next(1, options.Count);

        await locator.SelectOptionAsync(new SelectOptionValue { Index = randomIndex });

        s_log.Debug($"Dropdown selected random index: {randomIndex}");

        return true;
    }

    public static async Task<bool> IsDisabledAsync(this ILocator locator)
    {
        var isDisabled = !await locator.IsEnabledAsync();
        return isDisabled;
    }

    /// <summary>
    /// Checks if the element is visible within the specified timeout. Default timeout is 5 seconds.
    /// </summary>
    /// <param name="locator">The locator to check.</param>
    /// <param name="timeout">Optional timeout. Defaults to 5 seconds.</param>
    /// <returns>True if element is visible within the timeout, false otherwise.</returns>
    public static async Task<bool> IsElementVisibleAsync(this ILocator locator, TimeSpan? timeout = null)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions {
                State = WaitForSelectorState.Visible,
                Timeout = (float)(timeout ?? TimeSpan.FromSeconds(5)).TotalMilliseconds
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
