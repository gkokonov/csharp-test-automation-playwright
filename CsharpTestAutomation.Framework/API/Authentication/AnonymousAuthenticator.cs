using RestSharp;
using RestSharp.Authenticators;

namespace CsharpTestAutomation.Framework.API.Authentication;

/// <summary>
/// A no-op RestSharp <see cref="IAuthenticator"/> that applies no authentication. Assign
/// <see cref="Instance"/> to <see cref="RestRequest.Authenticator"/> to send a single request
/// without authentication, overriding any client-wide authenticator for that request only.
/// </summary>
/// <remarks>
/// RestSharp resolves the authenticator per request as <c>request.Authenticator ?? client.Authenticator</c>,
/// so leaving <see cref="RestRequest.Authenticator"/> as <see langword="null"/> does <b>not</b> suppress a
/// client-wide authenticator — it falls back to it. Assigning this non-null no-op authenticator is therefore
/// the correct way to issue an unauthenticated call from an otherwise-authenticated client (for example,
/// negative tests asserting that no <c>Authorization</c> header is sent).
/// </remarks>
public sealed class AnonymousAuthenticator : IAuthenticator
{
    /// <summary>Shared, thread-safe singleton instance.</summary>
    public static readonly AnonymousAuthenticator Instance = new();

    private AnonymousAuthenticator()
    {
    }

    /// <inheritdoc />
    public ValueTask Authenticate(IRestClient client, RestRequest request, CancellationToken cancellationToken = default)
        => default;
}
