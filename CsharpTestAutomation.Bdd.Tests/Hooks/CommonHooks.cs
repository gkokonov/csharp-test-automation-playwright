using CsharpTestAutomation.Bdd.Tests.Lifecycle;
using CsharpTestAutomation.Framework.Common;
using Reqnroll;
using Reqnroll.BoDi;

namespace CsharpTestAutomation.Bdd.Tests.Hooks;

[Binding]
public sealed class CommonHooks(IObjectContainer container)
{
    [BeforeScenario(Order = 0)]
    public void RegisterCommonState()
    {
        var cleanup = new ScenarioCleanupActions();
        container.RegisterInstanceAs(cleanup, dispose: false);
        container.RegisterInstanceAs(new ScenarioLifecycle(cleanup), dispose: false);
    }
}
