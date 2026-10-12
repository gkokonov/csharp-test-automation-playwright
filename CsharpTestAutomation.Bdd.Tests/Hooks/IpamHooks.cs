using CsharpTestAutomation.Bdd.Tests.Api.Clients;
using CsharpTestAutomation.Bdd.Tests.Authentication;
using CsharpTestAutomation.Bdd.Tests.Context;
using CsharpTestAutomation.Bdd.Tests.Lifecycle;
using CsharpTestAutomation.Framework.API.Clients;
using Reqnroll;
using Reqnroll.BoDi;

namespace CsharpTestAutomation.Bdd.Tests.Hooks;

[Binding]
public sealed class IpamHooks(IObjectContainer container, ScenarioLifecycle lifecycle)
{
    [BeforeScenario("Prefix", Order = 5)]
    public void RegisterPrefixState() => container.RegisterInstanceAs(new PrefixScenarioState(), dispose: false);

    [BeforeScenario("IpAddress", Order = 5)]
    public void RegisterIpAddressState() => container.RegisterInstanceAs(new IpAddressScenarioState(), dispose: false);

    [BeforeScenario("Ipam", Order = 25)]
    public async Task RegisterDependenciesAsync()
    {
        BddRunResources resources = BddRunResources.Current;
        var factory = new RestClientFactory(resources.Configuration.Api);
        var client = new IpamApiClient(factory, new NetBoxTokenAuthenticator(resources.TokenSession));
        lifecycle.AddRelease("IPAM API client", () =>
        {
            client.Dispose();
            return Task.CompletedTask;
        });
        container.RegisterInstanceAs(client, dispose: false);
        container.RegisterInstanceAs(await resources.GetIpamRepositoryAsync(), dispose: false);
    }
}
