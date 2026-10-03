using Allure.Net.Commons.Attributes;
using Allure.NUnit;
using AwesomeAssertions;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.UI;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Framework.Test.Tests.UI;

[TestFixture]
[AllureNUnit]
[AllureFeature("PlaywrightBrowserFactory")]
[AllureSuite("Framework Tests")]
public class PlaywrightContextOptionsTests
{
    [Test]
    public void Verify_ConfiguredViewportAndCredentialsApplied_When_DeviceIsNotSet()
    {
        var configuration = new CoreConfiguration {
            ViewportSize = new ViewportSize { Width = 800, Height = 600 },
            BypassCSP = true,
            RecordVideoEnabled = true,
            RecordDir = "videos"
        };
        var credentials = new HttpCredentials { Username = "user", Password = "pass" };

        BrowserNewContextOptions options = PlaywrightBrowserFactory.CreateContextOptions(
            configuration,
            deviceOptions: null,
            storageState: "{\"cookies\":[]}",
            credentials);

        options.ViewportSize!.Width.Should().Be(800);
        options.ViewportSize.Height.Should().Be(600);
        options.UserAgent.Should().BeNull();
        options.StorageState.Should().Be("{\"cookies\":[]}");
        options.HttpCredentials.Should().BeSameAs(credentials);
        options.BypassCSP.Should().BeTrue();
        options.IgnoreHTTPSErrors.Should().BeTrue();
        options.RecordVideoDir.Should().Be("videos");
    }

    [Test]
    public void Verify_FileNotFoundExceptionThrown_When_StorageStateFileIsMissing()
    {
        Action act = () => PlaywrightBrowserFactory.CreateContextOptions(
            new CoreConfiguration(),
            deviceOptions: null,
            storageState: "missing\\state.json",
            httpCredentials: null);

        act.Should().Throw<FileNotFoundException>().WithMessage("*missing*state.json*");
    }

    [Test]
    public void Verify_DeviceFieldsCopiedAndSessionOptionsKept_When_DeviceIsSet()
    {
        var configuration = new CoreConfiguration {
            ViewportSize = new ViewportSize { Width = 800, Height = 600 },
            BypassCSP = true,
            RecordVideoEnabled = false
        };
        var device = new BrowserNewContextOptions {
            UserAgent = "device-agent",
            ViewportSize = new ViewportSize { Width = 390, Height = 844 },
            IsMobile = true,
            HasTouch = true,
            DeviceScaleFactor = 3
        };
        var credentials = new HttpCredentials { Username = "user", Password = "pass" };

        BrowserNewContextOptions options = PlaywrightBrowserFactory.CreateContextOptions(
            configuration,
            device,
            "{\"cookies\":[]}",
            credentials);

        options.UserAgent.Should().Be("device-agent");
        options.ViewportSize!.Width.Should().Be(390);
        options.ViewportSize.Height.Should().Be(844);
        options.IsMobile.Should().BeTrue();
        options.HasTouch.Should().BeTrue();
        options.DeviceScaleFactor.Should().Be(3);
        options.StorageState.Should().Be("{\"cookies\":[]}");
        options.HttpCredentials.Should().BeSameAs(credentials);
        options.BypassCSP.Should().BeTrue();
        options.IgnoreHTTPSErrors.Should().BeTrue();
        options.RecordVideoDir.Should().BeNull();

        device.StorageState.Should().BeNull();
        device.IgnoreHTTPSErrors.Should().BeNull();
        device.BypassCSP.Should().BeNull();
        device.HttpCredentials.Should().BeNull();
    }
}
