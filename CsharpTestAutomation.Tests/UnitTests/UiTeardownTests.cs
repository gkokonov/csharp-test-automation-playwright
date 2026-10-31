using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using AwesomeAssertions;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.UI;
using CsharpTestAutomation.Tests.Tests;

namespace CsharpTestAutomation.Tests.UnitTests;

[TestFixture]
[NonParallelizable]
[AllureNUnit]
[AllureSuite("Local infrastructure")]
[AllureFeature("UI teardown")]
public class UiTeardownTests
{
    [OneTimeTearDown]
    public static Task ReleasePlaywrightAsync() => PlaywrightBrowserFactory.DisposeAllAsync();

    [TestCase(false)]
    [TestCase(true)]
    [AllureStory("Data cleanup failures retain traces before releasing the browser")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("Automation Team")]
    public async Task Verify_FailureOnlyTraceRetention_When_DataCleanupFails(bool cleanupFails)
    {
        // Arrange
        CoreConfiguration configuration = AppConfiguration<CoreConfiguration>.Instance.Settings;
        bool originalTrace = configuration.TraceEnabled;
        bool originalFailureTrace = configuration.TraceOnFailure;
        string originalDirectory = configuration.TraceDir;
        string directory = Path.Combine(Path.GetTempPath(), $"ui-teardown-{Guid.NewGuid():N}");
        configuration.TraceEnabled = false;
        configuration.TraceOnFailure = true;
        configuration.TraceDir = directory;
        var probe = new UiProbe();
        try
        {
            await probe.SetUpAsync();
            probe.RegisterCleanup(cleanupFails);

            // Act
            if (cleanupFails)
            {
                await Assert.ThatAsync(probe.TearDownAsync, Throws.TypeOf<AssertionException>());
            }
            else
            {
                await probe.TearDownAsync();
            }

            // Assert
            string[] traces = Directory.Exists(directory) ? Directory.GetFiles(directory, "*.zip") : [];
            traces.Should().HaveCount(cleanupFails ? 1 : 0);
            probe.PageClosed.Should().BeTrue();
        }
        finally
        {
            try
            {
                await probe.TearDownAsync();
            }
            finally
            {
                configuration.TraceEnabled = originalTrace;
                configuration.TraceOnFailure = originalFailureTrace;
                configuration.TraceDir = originalDirectory;
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }
    }

    private sealed class UiProbe : UiTestBase
    {
        private Microsoft.Playwright.IPage? _observedPage;

        public bool PageClosed => _observedPage?.IsClosed == true;

        public void RegisterCleanup(bool fails)
        {
            _observedPage = Page;
            ScenarioCleanupActions.AddCleanUpAction(() =>
            {
                if (fails)
                {
                    throw new InvalidOperationException("Data cleanup failed.");
                }
            });
        }
    }
}
