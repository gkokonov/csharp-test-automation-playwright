using CsharpTestAutomation.Tests.UI.Components.MSEntraID;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Tests.UI.Pages.MSEntraID;

public class MSLoginPage(IPage page) : BaseUIPage(page)
{
    private readonly ILocator _accountInput = page.Locator("[name='loginfmt']");
    private readonly ILocator _nextButton = page.GetByRole(AriaRole.Button, new() { Name = "Next" });
    private readonly SecondaryLoginForm _secondaryLoginForm = new(page, page.Locator("body"));

    protected override ILocator PageReadyLocator => _accountInput;

    public async Task OpenAsync() => await Page.GotoAsync("https://login.microsoftonline.com/");

    public async Task EnterAccountAsync(string email) => await _accountInput.FillAsync(email);

    public async Task ClickNextAsync() => await _nextButton.ClickAsync();

    public async Task HandleSecondaryLoginAsync(string username, string password) =>
        await _secondaryLoginForm.SignInAsync(username, password);
}
