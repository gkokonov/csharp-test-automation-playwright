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

        try
        {
            await OnTearDownAsync();
        }
        finally
        {
            await CleanUpContainerAsync();
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

    private async Task CleanUpContainerAsync()
    {
        try
        {
            // Execute cleanup actions in LIFO order
            if (TestContainer.HasService<ScenarioCleanupActions>())
            {
                await TestContainer.Get<ScenarioCleanupActions>()!.CleanUpAsync();
            }

            // Dispose and clear services
            s_log.Debug("Dispose Test Container and its services");
            await TestContainer.DisposeServicesAsync();
            TestContainer.Clear();
        }
        catch (Exception ex)
        {
            s_log.Error(ex, "Error during test tear down");
            throw;
        }
    }
}
