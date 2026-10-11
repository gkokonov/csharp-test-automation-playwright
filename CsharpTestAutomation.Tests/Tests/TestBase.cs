using Allure.NUnit;
using Allure.NUnit.Attributes;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Configurations;
using NLog;

namespace CsharpTestAutomation.Tests.Tests;

/// <summary>
/// Base class for all test fixtures in the framework. Provides common functionality for both UI
/// and API tests.
/// </summary>
[TestFixture]
[AllureNUnit]
public abstract class TestBase
{
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

    protected static readonly ExtendedConfiguration s_configuration = AppConfiguration<ExtendedConfiguration>.Instance.Settings;

    protected TestContainer TestContainer { get; } = new TestContainer();

    protected ScenarioCleanupActions ScenarioCleanupActions =>
        TestContainer.Get<ScenarioCleanupActions>() ?? throw new InvalidOperationException("ScenarioCleanupActions not registered.");

    [OneTimeSetUp]
    public virtual Task OneTimeSetUpAsync() => Task.CompletedTask;

    [OneTimeTearDown]
    public virtual Task OneTimeTearDownAsync() => Task.CompletedTask;

    [SetUp]
    [AllureBefore("Init test")]
    public async Task SetUpAsync()
    {
        s_log.Info($"{Environment.NewLine}----- Initializing test '{TestIdentifier.GetTestId()}' -----");

        // Init and register cleanup actions
        TestContainer.Register(new ScenarioCleanupActions());

        await OnSetUpAsync();
    }

    [TearDown]
    [AllureAfter("End test")]
    public async Task TearDownAsync()
    {
        s_log.Info($"{Environment.NewLine}----- Ending test '{TestIdentifier.GetTestId()}' -----");

        bool testFailed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed;
        List<Exception> errors = [];
        try
        {
            await AttemptTeardownAsync(OnTearDownAsync, errors);
            if (TestContainer.Get<ScenarioCleanupActions>() is { } cleanup)
            {
                await AttemptTeardownAsync(() => cleanup.CleanUpAsync(), errors);
            }

            await AttemptTeardownAsync(TestContainer.DisposeServicesAsync, errors);
        }
        finally
        {
            TestContainer.Clear();
        }

        if (!testFailed && errors.Count > 0)
        {
            var error = new AggregateException("Test teardown failed.", errors);
            throw new AssertionException(error.Message, error);
        }
    }

    /// <summary>
    /// Hook for per-test setup specific to a test type (UI, API). Override to add behavior; no base
    /// call required.
    /// </summary>
    protected virtual Task OnSetUpAsync() => Task.CompletedTask;

    /// <summary>
    /// Hook for per-test teardown specific to a test type (UI, API). Override to add behavior; no
    /// base call required.
    /// </summary>
    protected virtual Task OnTearDownAsync() => Task.CompletedTask;

    private static async Task AttemptTeardownAsync(Func<Task> action, List<Exception> errors)
    {
        try { await action(); }
        catch (Exception ex)
        {
            errors.Add(ex);
            s_log.Error(ex, "Test teardown failed.");
            TestContext.Error.WriteLine($"Secondary teardown error: {ex}");
        }
    }
}
