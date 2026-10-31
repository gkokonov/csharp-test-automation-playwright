using Allure.Net.Commons.Attributes;
using Allure.NUnit;
using AwesomeAssertions;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.Common.Reporting.Allure;
using CsharpTestAutomation.Framework.UI;
using Microsoft.Playwright;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CsharpTestAutomation.Framework.Test.Tests.UI;

// This fixture calls DisposeAllAsync, which closes every browser and the shared Playwright
// instance. It must not run beside another fixture.
[TestFixture]
[NonParallelizable]
[AllureNUnit]
[AllureFeature("PlaywrightBrowserFactory")]
[AllureSuite("Framework Tests")]
public class PlaywrightBrowserFactoryTests
{
    [TearDown]
    [AllureAfter("Dispose per-test browser resources")]
    public async Task TearDownAsync()
    {
        await PlaywrightBrowserFactory.DisposeContextAsync();
        await PlaywrightBrowserFactory.DisposeBrowserAsync();
    }

    [OneTimeTearDown]
    [AllureAfter("Dispose all Playwright resources (safety net)")]
    public async Task OneTimeTearDownAsync() => await PlaywrightBrowserFactory.DisposeAllAsync();

    [Test]
    public async Task Verify_BrowserInstanceCreated_When_InitializeAsyncIsCalled()
    {
        // Act
        await PlaywrightBrowserFactory.InitializeAsync();

        // Assert
        IBrowser browser = PlaywrightBrowserFactory.GetCurrentBrowser();
        browser.Should().NotBeNull();
        browser.IsConnected.Should().BeTrue();
    }

    [Test]
    public async Task Verify_BrowserContextCreated_When_CreateContextAsyncIsCalled()
    {
        // Arrange
        await PlaywrightBrowserFactory.InitializeAsync();

        // Act
        IBrowserContext context = await PlaywrightBrowserFactory.CreateContextAsync();

        // Assert
        context.Should().NotBeNull();

        IBrowserContext retrievedContext = PlaywrightBrowserFactory.GetCurrentContext();
        retrievedContext.Should().BeSameAs(context);
    }

    [Test]
    public async Task Verify_PageCreated_When_CreatePageAsyncIsCalled()
    {
        // Arrange
        await PlaywrightBrowserFactory.InitializeAsync();
        await PlaywrightBrowserFactory.CreateContextAsync();

        // Act
        IPage page = await PlaywrightBrowserFactory.CreatePageAsync();
        await page.SetContentAsync("<h1>Local page</h1>");
        await AllureExtensions.CaptureScreenshotAsync(page, "LocalPageScreenshot");

        // Assert
        page.Should().NotBeNull();
        page.IsClosed.Should().BeFalse();
    }

    [Test]
    public async Task Verify_ContextClosedAndRemoved_When_DisposeContextAsyncIsCalled()
    {
        // Arrange
        await PlaywrightBrowserFactory.InitializeAsync();
        await PlaywrightBrowserFactory.CreateContextAsync();

        // Act
        await PlaywrightBrowserFactory.DisposeContextAsync();

        // Assert
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = PlaywrightBrowserFactory.GetCurrentContext();
        });

        ex.Message.Should().Contain("not initialized");
    }

    [Test]
    public async Task Verify_BrowserClosedAndRemoved_When_DisposeBrowserAsyncIsCalled()
    {
        // Arrange
        await PlaywrightBrowserFactory.InitializeAsync();

        // Act
        await PlaywrightBrowserFactory.DisposeBrowserAsync();

        // Assert
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = PlaywrightBrowserFactory.GetCurrentBrowser();
        });

        ex.Message.Should().Contain("not initialized");
    }

    [Test]
    public async Task Verify_AllResourcesClosed_When_DisposeAllAsyncIsCalled()
    {
        // Arrange
        await PlaywrightBrowserFactory.InitializeAsync();
        await PlaywrightBrowserFactory.CreateContextAsync();

        // Act
        await PlaywrightBrowserFactory.DisposeAllAsync();

        // Assert
        InvalidOperationException exBrowser = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = PlaywrightBrowserFactory.GetCurrentBrowser();
        });

        InvalidOperationException exContext = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = PlaywrightBrowserFactory.GetCurrentContext();
        });

        exBrowser.Message.Should().Contain("not initialized");
        exContext.Message.Should().Contain("not initialized");
    }

    [Test]
    public void Verify_InvalidOperationExceptionThrown_When_BrowserIsNotInitialized()
    {
        // Act & Assert
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = PlaywrightBrowserFactory.GetCurrentBrowser();
        });

        ex.Message.Should().Contain("not initialized");
    }

    [Test]
    public async Task Verify_InvalidOperationExceptionThrown_When_ContextIsNotInitialized()
    {
        // Arrange
        await PlaywrightBrowserFactory.InitializeAsync();

        // Act & Assert
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = PlaywrightBrowserFactory.GetCurrentContext();
        });

        ex.Message.Should().Contain("not initialized");
    }

    [Test]
    public async Task Verify_CookiesPreserved_When_ContextUsesStorageState()
    {
        // Arrange
        string testCookieName = "testCookie";
        string testCookieValue = "cookieValue123";

        await PlaywrightBrowserFactory.InitializeAsync();

        await PlaywrightBrowserFactory.CreateContextAsync();
        IBrowserContext firstContext = PlaywrightBrowserFactory.GetCurrentContext();
        await firstContext.AddCookiesAsync([
                new Cookie
                {
                    Name = testCookieName,
                    Value = testCookieValue,
                    Domain = "local.test",
                    Path = "/",
                }
            ]);

        string storageState = await firstContext.StorageStateAsync();

        await PlaywrightBrowserFactory.DisposeContextAsync();

        // Act
        IBrowserContext secondContext = await PlaywrightBrowserFactory.CreateContextAsync(storageState);

        // Assert
        IReadOnlyList<BrowserContextCookiesResult> cookies = await secondContext.CookiesAsync("https://local.test");
        BrowserContextCookiesResult? testCookie = cookies.FirstOrDefault(c => c.Name == testCookieName);

        testCookie.Should().NotBeNull("Cookie should be preserved in the new context");
        testCookie.Value.Should().Be(testCookieValue);
    }

    [Test]
    public async Task Verify_ProtectedSiteAuthenticationSucceeds_When_ContextUsesHttpCredentials()
    {
        // Arrange
        using WireMockServer server = WireMockServer.Start();
        server.Given(Request.Create().WithPath("/protected").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(401).WithHeader("WWW-Authenticate", "Basic realm=\"local\""));
        server.Given(Request.Create().WithPath("/protected").UsingGet()
                .WithHeader("Authorization", "Basic dXNlcjpwYXNzd2Q="))
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("Authenticated locally"));
        CoreConfiguration config = AppConfiguration<CoreConfiguration>.Instance.Settings;
        HttpCredentials? originalCredentials = config.HttpCredentials;
        config.HttpCredentials = new() { Username = "user", Password = "passwd", Origin = server.Url };
        try
        {
            await PlaywrightBrowserFactory.InitializeAsync();
            await PlaywrightBrowserFactory.CreateContextAsync();
            IPage page = await PlaywrightBrowserFactory.CreatePageAsync();

            // Act
            IResponse? response = await page.GotoAsync($"{server.Url}/protected");

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be(200);
            string content = await page.ContentAsync();
            content.Should().Contain("Authenticated locally");
        }
        finally
        {
            config.HttpCredentials = originalCredentials;
        }
    }
}
