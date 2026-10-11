using AwesomeAssertions;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.UI;
using Microsoft.Playwright;
using Moq;

namespace CsharpTestAutomation.Framework.Test.Tests.UI;

[TestFixture]
public class BrowserContextDiagnosticsTests
{
    [TestCase(false, false, false, false, false)]
    [TestCase(false, false, true, false, false)]
    [TestCase(false, true, false, true, false)]
    [TestCase(false, true, true, true, true)]
    [TestCase(true, false, false, true, true)]
    [TestCase(true, false, true, true, true)]
    [TestCase(true, true, false, true, true)]
    [TestCase(true, true, true, true, true)]
    public async Task Verify_TraceRetention_When_ConfigurationAndOutcomeVary(
        bool enabled, bool onFailure, bool failed, bool record, bool retain)
    {
        var tracing = new Mock<ITracing>();
        var context = new Mock<IBrowserContext>();
        context.SetupGet(x => x.Tracing).Returns(tracing.Object);
        string directory = Path.Combine(Path.GetTempPath(), $"trace-test-{Guid.NewGuid():N}");
        var configuration = new CoreConfiguration { TraceEnabled = enabled, TraceOnFailure = onFailure, TraceDir = directory };
        using var diagnostics = new BrowserContextDiagnostics(context.Object, configuration);
        try
        {
            await diagnostics.StartTracingAsync();
            string? path = await diagnostics.StopTracingAsync(failed, "case:with/invalid?characters");
            string? secondPath = await diagnostics.StopTracingAsync(failed, "same case");

            secondPath.Should().BeNull();
            (path is not null).Should().Be(retain);
            if (path is not null)
            {
                Path.GetDirectoryName(path).Should().Be(directory);
                Path.GetFileName(path).Should().EndWith(".zip");
                Path.GetFileName(path).IndexOfAny(Path.GetInvalidFileNameChars()).Should().Be(-1);
            }

            tracing.Verify(x => x.StartAsync(It.Is<TracingStartOptions>(o => o.Screenshots == true && o.Snapshots == true && o.Sources == true)), record ? Times.Once() : Times.Never());
            tracing.Verify(x => x.StopAsync(It.Is<TracingStopOptions>(o => o.Path == path)), record ? Times.Once() : Times.Never());
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory);
            }
        }
    }

    [Test]
    public void Verify_ConsoleAndPageErrorsRemainIsolated_When_PopupsEmitEvents()
    {
        var first = new Mock<IBrowserContext>();
        var second = new Mock<IBrowserContext>();
        var popup = new Mock<IPage>();
        popup.SetupGet(x => x.Url).Returns("https://local.test/popup");
        var message = new Mock<IConsoleMessage>();
        message.SetupGet(x => x.Page).Returns(popup.Object);
        message.SetupGet(x => x.Type).Returns("error");
        message.SetupGet(x => x.Text).Returns("Console failure");
        var error = new Mock<IWebError>();
        error.SetupGet(x => x.Page).Returns(popup.Object);
        error.SetupGet(x => x.Error).Returns("Uncaught failure");
        var configuration = new CoreConfiguration { CaptureBrowserLogs = true };
        using var firstDiagnostics = new BrowserContextDiagnostics(first.Object, configuration);
        using var secondDiagnostics = new BrowserContextDiagnostics(second.Object, configuration);

        first.Raise(x => x.Console += null, first.Object, message.Object);
        first.Raise(x => x.WebError += null, first.Object, error.Object);

        firstDiagnostics.GetLogs().Should().BeEquivalentTo(new[] {
            "[https://local.test/popup] console.error: Console failure",
            "[https://local.test/popup] JavaScript error: Uncaught failure"
        });
        secondDiagnostics.GetLogs().Should().BeEmpty();
        firstDiagnostics.Dispose();
        first.Raise(x => x.Console += null, first.Object, message.Object);
        firstDiagnostics.GetLogs().Should().HaveCount(2);
    }

    [Test]
    public async Task Verify_ContextCloseAttemptedAndErrorsAggregated_When_TraceAndCloseFail()
    {
        var tracing = new Mock<ITracing>();
        var context = new Mock<IBrowserContext>();
        context.SetupGet(x => x.Tracing).Returns(tracing.Object);
        tracing.Setup(x => x.StopAsync(It.IsAny<TracingStopOptions>())).ThrowsAsync(new IOException("trace failed"));
        context.Setup(x => x.CloseAsync(It.IsAny<BrowserContextCloseOptions>())).ThrowsAsync(new IOException("close failed"));
        using var diagnostics = new BrowserContextDiagnostics(context.Object, new CoreConfiguration { TraceOnFailure = true });
        await diagnostics.StartTracingAsync();

        Func<Task> release = () => diagnostics.CloseAsync("test");
        AggregateException error = (await release.Should().ThrowAsync<AggregateException>()).Which;

        error.InnerExceptions.Select(x => x.Message).Should().Equal("trace failed", "close failed");
        context.Verify(x => x.CloseAsync(It.IsAny<BrowserContextCloseOptions>()), Times.Once());
    }

    [Test]
    public async Task Verify_TraceNotStopped_When_TraceStartFails()
    {
        var tracing = new Mock<ITracing>();
        var context = new Mock<IBrowserContext>();
        context.SetupGet(x => x.Tracing).Returns(tracing.Object);
        tracing.Setup(x => x.StartAsync(It.IsAny<TracingStartOptions>())).ThrowsAsync(new IOException("start failed"));
        using var diagnostics = new BrowserContextDiagnostics(context.Object, new CoreConfiguration { TraceOnFailure = true });
        Func<Task> start = diagnostics.StartTracingAsync;
        await start.Should().ThrowAsync<IOException>();

        await diagnostics.CloseAsync("partial setup");

        tracing.Verify(x => x.StopAsync(It.IsAny<TracingStopOptions>()), Times.Never());
        context.Verify(x => x.CloseAsync(It.IsAny<BrowserContextCloseOptions>()), Times.Once());
    }
}
