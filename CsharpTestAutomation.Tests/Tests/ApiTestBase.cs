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

    protected static T RequireDbData<T>(object? candidate, string missingDataMessage)
    {
        if (candidate is T value)
        {
            return value;
        }

        Assert.Inconclusive(missingDataMessage);
        throw new InvalidOperationException("Assert.Inconclusive returned unexpectedly.");
    }

    protected static T RequireDbData<T>(IReadOnlyCollection<T>? candidates, string missingDataMessage)
    {
        if (candidates is null || candidates.Count == 0)
        {
            Assert.Inconclusive(missingDataMessage);
            throw new InvalidOperationException("Assert.Inconclusive returned unexpectedly.");
        }

        return candidates.ElementAt(Random.Shared.Next(candidates.Count));
    }

    protected override Task OnSetUpAsync()
    {
        RestClientFactory = new RestClientFactory(s_configuration.Api);
        return Task.CompletedTask;
    }
}
