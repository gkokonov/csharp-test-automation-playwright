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

    protected IAuthenticator NetBoxAuthenticator => NetBoxTokenAuthenticator.Default;

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

    /// <summary>
    /// Registers a disposable typed API client for teardown. Call once per client, from an
    /// overridden <c>OnSetUpAsync</c>, right after constructing it. <see cref="TestBase"/> disposes
    /// every registered service only *after* <see cref="TestBase.ScenarioCleanupActions"/> has run
    /// (see <c>TestBase.CleanUpContainerAsync</c>), so a client registered this way stays usable for
    /// any delete call a cleanup action performs. Retrieve it later with <see cref="GetClient{T}"/>.
    /// </summary>
    protected void RegisterClient<T>(T client) where T : class, IDisposable => TestContainer.Register(client);

    /// <summary>
    /// Retrieves a typed API client previously registered with <see cref="RegisterClient{T}"/>.
    /// Expose this through a fixture-level property
    /// (<c>private FooApiClient FooClient => GetClient&lt;FooApiClient&gt;();</c>) instead of caching
    /// it in a plain field — the teardown-ordering guarantee above depends on the client living in
    /// the container, not a fixture field.
    /// </summary>
    protected T GetClient<T>() where T : class =>
        TestContainer.Get<T>() ?? throw new InvalidOperationException($"{typeof(T).Name} not registered. Call RegisterClient in OnSetUpAsync.");

    protected override Task OnSetUpAsync()
    {
        RestClientFactory = new RestClientFactory(s_configuration.Api);
        return Task.CompletedTask;
    }
}
