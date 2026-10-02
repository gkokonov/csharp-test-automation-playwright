using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Tests.Authentication;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Tests.Tests.UI.NetBox;

public abstract class NetBoxUiTestBase : UiTestBase
{
    protected RestClientFactory RestClientFactory { get; private set; } = null!;

    protected IAuthenticator NetBoxAuthenticator => NetBoxTokenAuthenticator.Default;

    protected void RegisterClient<T>(T client) where T : class, IDisposable => TestContainer.Register(client);

    protected T GetClient<T>() where T : class =>
        TestContainer.Get<T>() ?? throw new InvalidOperationException($"{typeof(T).Name} not registered. Call RegisterClient in OnSetUpAsync.");

    protected override async Task OnSetUpAsync()
    {
        await InitializePlaywrightEnvironmentAsync(NetBoxUiSetupFixture.StorageStatePath);
        RestClientFactory = new RestClientFactory(s_configuration.Api);
    }
}
