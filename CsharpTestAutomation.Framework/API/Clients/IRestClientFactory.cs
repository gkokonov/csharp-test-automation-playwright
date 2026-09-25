#nullable enable

using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Framework.API.Clients;

/// <summary>
/// Creates configured <see cref="IRestClient"/> instances for named API services.
/// </summary>
public interface IRestClientFactory
{
    /// <summary>
    /// Creates an <see cref="IRestClient"/> for the service identified by <paramref name="serviceName"/>,
    /// applying <paramref name="authenticator"/> as the client-wide authenticator when supplied.
    /// </summary>
    /// <param name="serviceName">Logical service name; must match a key under <c>Api.Services</c>.</param>
    /// <param name="authenticator">Optional RestSharp authenticator applied to every request made by the client.</param>
    IRestClient Create(string serviceName, IAuthenticator? authenticator = null);
}
