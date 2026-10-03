using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using AwesomeAssertions;
using CsharpTestAutomation.Tests.UI.Pages.MSEntraID;

namespace CsharpTestAutomation.Tests.Tests.UI;

/// <summary>
/// Reference example for UI tests. Shows how <see cref="UiTestBase"/> (which owns the Playwright
/// browser/context/page lifecycle plus failure screenshots and traces) composes with the page-object
/// model: <c>GetPage&lt;T&gt;()</c> to obtain a page bound to the current <see cref="IPage"/>,
/// intent-revealing page actions, and the enforced readiness contract
/// (<c>WaitUntilLoadedAsync</c> / <c>IsLoadedAsync</c>). Copy this as a template for real UI tests.
/// The single test is ignored because it targets placeholder credentials and a placeholder URL.
/// </summary>
[AllureSuite("UI")]
[AllureFeature("Microsoft Entra ID Sign-in")]
public class ExampleUiTests : UiTestBase
{
    // Placeholder example data — real tests should source these from secure configuration / test data.
    private const string AccountEmail = "example.user@contoso.com";

    private const string FederatedUsername = "CONTOSO\\example.user";
    private const string FederatedPassword = "placeholder-not-a-real-secret";

    [Test]
    [Category("PlanId:0,TestSuiteId:0,TestCaseId:0")]
    [Ignore("Example template only: placeholder credentials and a placeholder sign-in URL. Enable once a real environment and landing page object are wired.")]
    [AllureStory("User signs in through the Microsoft Entra ID login page")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("Automation Team")]
    [AllureDescription("End-to-end UI workflow: open the login page, enter the account, handle the federated (secondary) login form, and verify the login page is dismissed after a successful sign-in.")]
    public async Task Verify_LoginPageDismissed_When_AccountIsValid()
    {
        // Arrange
        MSLoginPage loginPage = GetPage<MSLoginPage>();

        // Act
        await loginPage.OpenAsync();
        await loginPage.WaitUntilLoadedAsync();

        await loginPage.EnterAccountAsync(AccountEmail);
        await loginPage.ClickNextAsync();
        await loginPage.HandleSecondaryLoginAsync(FederatedUsername, FederatedPassword);

        // Assert
        var loginPageStillVisible = await loginPage.IsLoadedAsync();
        loginPageStillVisible.Should().BeFalse("a successful sign-in should navigate away from the login page");
    }
}
