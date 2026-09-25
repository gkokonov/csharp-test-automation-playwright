using CsharpTestAutomation.Framework.API.Clients;
using CsharpTestAutomation.Tests.Authentication;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Tests.Tests;

/// <summary>
/// Base class for API tests using RestSharp.
/// </summary>
[Category("API")]
public abstract class ApiTestBase : TestBase
{
    protected RestClientFactory RestClientFactory { get; private set; } = null!;

    protected IAuthenticator BootstrapAuthenticator =>
        new JwtAuthenticator(BootstrapSession.Default.Token);

    protected override Task OnSetUpAsync()
    {
        RestClientFactory = new RestClientFactory(s_configuration.Api);
        return Task.CompletedTask;
    }
}
