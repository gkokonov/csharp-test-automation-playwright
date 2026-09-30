namespace CsharpTestAutomation.Framework.Test.Tests.Common;

[TestFixture]
[Category("ScenarioCleanupActionsTests")]
public class ScenarioCleanupActionsTests
{
    [Test]
    public async Task CleanUpAsync_MixedSyncAndAsyncActions_RunsInReverseRegistrationOrder()
    {
        var cleanup = new CsharpTestAutomation.Framework.Common.ScenarioCleanupActions();
        var executed = new List<string>();
        cleanup.AddCleanUpAction(() => { executed.Add("async-1"); return Task.CompletedTask; });
        cleanup.AddCleanUpAction(() => executed.Add("sync-2"));
        cleanup.AddCleanUpAction(() => { executed.Add("async-3"); return Task.CompletedTask; });
        cleanup.AddCleanUpAction(() => executed.Add("sync-4"));

        await cleanup.CleanUpAsync();

        Assert.That(executed, Is.EqualTo(new[] { "sync-4", "async-3", "sync-2", "async-1" }));
    }

    [Test]
    public async Task CleanUpAsync_CalledTwice_RunsActionsOnce()
    {
        var cleanup = new CsharpTestAutomation.Framework.Common.ScenarioCleanupActions();
        var count = 0;
        cleanup.AddCleanUpAction(() => count++);

        await cleanup.CleanUpAsync();
        await cleanup.CleanUpAsync();

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task AddCleanUpAction_ConcurrentAdds_KeepsEveryAction()
    {
        var cleanup = new CsharpTestAutomation.Framework.Common.ScenarioCleanupActions();
        var count = 0;

        Parallel.For(0, 1_000, _ => cleanup.AddCleanUpAction(() => Interlocked.Increment(ref count)));
        await cleanup.CleanUpAsync();

        Assert.That(count, Is.EqualTo(1_000));
    }
}
