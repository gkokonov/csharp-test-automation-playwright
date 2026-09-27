using System.Net;
using CsharpTestAutomation.Framework.Common.Utilities.CustomExceptions;

namespace CsharpTestAutomation.Framework.Common.Extensions;

/// <summary>
/// HTTP helpers for verifying that an external dependency endpoint is reachable.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Sends a GET request to <paramref name="url"/> and throws <see cref="DependencyUnavailableException"/>
    /// when the endpoint is unreachable or returns a server-error status code. A 4xx response (for
    /// example an auth-gated API root) still counts as reachable — the server responded, it just
    /// rejected this particular unauthenticated request.
    /// </summary>
    /// <param name="client">The client used to send the request.</param>
    /// <param name="dependencyName">Human-readable name used in the exception message.</param>
    /// <param name="url">The absolute URL to check.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    public static async Task EnsureAvailableAsync(this HttpClient client, string dependencyName, string url, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new DependencyUnavailableException($"{dependencyName} URL must be an absolute URL.");
        }

        try
        {
            using HttpResponseMessage response = await client
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            if (response.StatusCode >= HttpStatusCode.InternalServerError)
            {
                throw new DependencyUnavailableException(
                    $"{dependencyName} dependency check failed with HTTP {(int)response.StatusCode} ({response.StatusCode}).");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new DependencyUnavailableException($"{dependencyName} dependency check failed for '{url}'.", ex);
        }
    }
}
