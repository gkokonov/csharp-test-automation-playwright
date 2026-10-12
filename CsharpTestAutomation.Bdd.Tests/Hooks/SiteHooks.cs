using CsharpTestAutomation.Bdd.Tests.Api.Clients;
using CsharpTestAutomation.Bdd.Tests.Authentication;
using CsharpTestAutomation.Bdd.Tests.Context;
using CsharpTestAutomation.Bdd.Tests.Lifecycle;
using CsharpTestAutomation.Framework.API.Clients;
using Reqnroll;
using Reqnroll.BoDi;

namespace CsharpTestAutomation.Bdd.Tests.Hooks;

[Binding]
[Scope(Tag = "Site")]
public sealed class SiteHooks(IObjectContainer container, ScenarioLifecycle lifecycle)
{
    [BeforeScenario(Order = 25)]
    public async Task RegisterDependenciesAsync()
    {
        container.RegisterInstanceAs(new SiteScenarioState(), dispose: false);
        BddRunResources resources = BddRunResources.Current;
        var factory = new RestClientFactory(resources.Configuration.Api);
        var client = new SitesApiClient(factory, new NetBoxTokenAuthenticator(resources.TokenSession));
        lifecycle.AddRelease("Site API client", () =>
        {
            client.Dispose();
            return Task.CompletedTask;
        });
        container.RegisterInstanceAs(client, dispose: false);
        container.RegisterInstanceAs(await resources.GetSitesRepositoryAsync(), dispose: false);
    }
}
