using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CsharpTestAutomation.Bdd.Tests.Lifecycle;
using CsharpTestAutomation.Framework.Common;

namespace CsharpTestAutomation.Bdd.Tests.UnitTests;

[TestFixture]
public class ScenarioLifecycleTests
{
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Verify_TraceReleaseSeesFailure_When_BodyOrDataCleanupFails(bool bodyFails, bool cleanupFails)
    {
        // Arrange
        var cleanup = new ScenarioCleanupActions();
        var lifecycle = new ScenarioLifecycle(cleanup);
        List<string> calls = [];
        bool? traceFailed = null;
        cleanup.AddCleanUpAction(() =>
        {
            calls.Add("cleanup");
            if (cleanupFails)
            {
                throw new InvalidOperationException("cleanup failed");
            }
        });
        lifecycle.AddRelease("client", () => { calls.Add("client"); return Task.CompletedTask; });
        lifecycle.AddRelease("browser", () => { calls.Add("browser"); return Task.CompletedTask; });
        lifecycle.AddRelease("context", () => { calls.Add("context"); return Task.CompletedTask; });
        lifecycle.AddRelease("trace", failed =>
        {
            traceFailed = failed;
            calls.Add("trace");
            return Task.CompletedTask;
        });

        // Act
        AggregateException? error = await lifecycle.CompleteAsync(
            bodyFails ? new InvalidOperationException("body failed") : null, (_, _) => { });
        await lifecycle.CompleteAsync(null, (_, _) => { });

        // Assert
        using (new AssertionScope())
        {
            traceFailed.Should().Be(bodyFails || cleanupFails);
            calls.Should().Equal("cleanup", "trace", "context", "browser", "client");
            (error is not null).Should().Be(!bodyFails && cleanupFails);
        }
    }

    [Test]
    public async Task Verify_AllResourcesReleased_When_TraceAndReportingFail()
    {
        // Arrange
        var lifecycle = new ScenarioLifecycle(new ScenarioCleanupActions());
        List<string> released = [];
        lifecycle.AddRelease("client", () => { released.Add("client"); return Task.CompletedTask; });
        lifecycle.AddRelease("context", () => { released.Add("context"); return Task.CompletedTask; });
        lifecycle.AddRelease("trace", _ => throw new IOException("trace failed"));

        // Act
        AggregateException? error = await lifecycle.CompleteAsync(null, (_, _) => throw new IOException("report failed"));

        // Assert
        error.Should().NotBeNull();
        error.InnerExceptions.Should().ContainSingle().Which.Message.Should().Be("trace failed");
        released.Should().Equal("context", "client");
    }
}
