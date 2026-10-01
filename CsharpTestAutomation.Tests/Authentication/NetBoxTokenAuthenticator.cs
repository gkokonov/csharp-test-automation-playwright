using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Tests.Authentication;

public sealed class NetBoxTokenAuthenticator(NetBoxSession session) : IAuthenticator
{
    /// <summary>
    /// Shared instance bound to <see cref="NetBoxSession.Default"/>. The single owner for this
    /// construction so API test fixtures (both <c>ApiTestBase</c>-derived and plain
    /// <c>[TestFixture]</c> ones) never restate it.
    /// </summary>
    public static NetBoxTokenAuthenticator Default => new(NetBoxSession.Default);

    public async ValueTask Authenticate(IRestClient client, RestRequest request, CancellationToken cancellationToken)
    {
        string token = await session.GetTokenAsync(cancellationToken);
        request.AddOrUpdateHeader("Authorization", $"Token {token}");
    }
}
