using CsharpTestAutomation.Bdd.Tests.Api.Clients;
using CsharpTestAutomation.Bdd.Tests.Authentication;
using CsharpTestAutomation.Bdd.Tests.Context;
using CsharpTestAutomation.Bdd.Tests.Lifecycle;
using CsharpTestAutomation.Framework.API.Clients;
using Reqnroll;
using Reqnroll.BoDi;

namespace CsharpTestAutomation.Bdd.Tests.Hooks;

[Binding]
[Scope(Tag = "Device")]
public sealed class DeviceHooks(IObjectContainer container, ScenarioLifecycle lifecycle)
{
    [BeforeScenario(Order = 25)]
    public async Task RegisterDependenciesAsync()
    {
        container.RegisterInstanceAs(new DeviceScenarioState(), dispose: false);
        BddRunResources resources = BddRunResources.Current;
        var factory = new RestClientFactory(resources.Configuration.Api);
        var authenticator = new NetBoxTokenAuthenticator(resources.TokenSession);
        RegisterClient(new SitesApiClient(factory, authenticator), "Site API client");
        RegisterClient(new ManufacturersApiClient(factory, authenticator), "Manufacturer API client");
        RegisterClient(new DeviceTypesApiClient(factory, authenticator), "Device type API client");
        RegisterClient(new DeviceRolesApiClient(factory, authenticator), "Device role API client");
        RegisterClient(new DevicesApiClient(factory, authenticator), "Device API client");
        container.RegisterInstanceAs(await resources.GetDevicesRepositoryAsync(), dispose: false);
    }

    private void RegisterClient<T>(T client, string name) where T : class, IDisposable
    {
        lifecycle.AddRelease(name, () =>
        {
            client.Dispose();
            return Task.CompletedTask;
        });
        container.RegisterInstanceAs(client, dispose: false);
    }
}
