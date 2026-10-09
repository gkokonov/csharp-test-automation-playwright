using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Bdd.Tests.Authentication;

public sealed class NetBoxTokenAuthenticator(NetBoxSession session) : IAuthenticator
{
    public async ValueTask Authenticate(IRestClient client, RestRequest request, CancellationToken cancellationToken)
    {
        string token = await session.GetTokenAsync(cancellationToken);
        request.AddOrUpdateHeader("Authorization", $"Token {token}");
    }
}
