using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Components.MSEntraID;

/// <summary>
/// Secondary (federated) sign-in form shown by some Entra ID tenants. Demonstrates the component
/// pattern: child locators are scoped to <see cref="BaseUIComponent.Root"/>, while page-level
/// prompts (such as "Stay signed in?") are read from the page directly. Compose this inside a page
/// object rather than inheriting from it.
/// </summary>
public class SecondaryLoginForm(IPage page, ILocator root) : BaseUIComponent(page, root)
{
    private readonly ILocator _usernameInput = root.Locator("#userNameInput");
    private readonly ILocator _passwordInput = root.Locator("#passwordInput");
    private readonly ILocator _submitButton = root.Locator("#submitButton");
    private readonly ILocator _yesButton = page.GetByRole(AriaRole.Button, new() { Name = "Yes" });

    public async Task SignInAsync(string username, string password)
    {
        await Expect(_usernameInput).ToBeVisibleAsync();
        await _usernameInput.FillAsync(username);
        await _passwordInput.FillAsync(password);
        await _submitButton.ClickAsync();
        await _yesButton.ClickAsync();
    }
}
