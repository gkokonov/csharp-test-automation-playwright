using System.Collections.Concurrent;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Tests.Configurations;
using CsharpTestAutomation.Tests.Configurations.Models;
using Microsoft.Identity.Client;

namespace CsharpTestAutomation.Tests.Integrations.Microsoft;

/// <summary>
/// Acquires and caches tokens from Microsoft Entra ID using MSAL.NET.
/// Tokens are keyed by the logical scope name defined in <c>EntraIdSettings.EntraIdScopes</c>.
/// </summary>
public static class EntraIdTokenService
{
    private static readonly ExtendedConfiguration s_configuration = AppConfiguration<ExtendedConfiguration>.Instance.Settings;

    private static readonly EntraIdConfigurationDTO s_entraIdSettings = s_configuration.EntraIdSettings
        ?? throw new InvalidOperationException(
            "EntraIdSettings configuration section is missing. Configure 'EntraIdSettings' in appsettings.");

    private static readonly string s_authority = $"https://login.microsoftonline.com/{s_entraIdSettings.EntraIdTenantId}";

    /// <summary>
    /// In-memory token cache keyed by logical scope name (e.g. <see cref="TokenScope.InvitationApi"/>).
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> s_tokenCache = new();

    /// <summary>
    /// Returns a cached token for the given scope key, acquiring a new one if not yet cached.
    /// </summary>
    /// <param name="scopeKey">
    /// Logical scope key matching an entry in <c>EntraIdSettings.EntraIdScopes</c>.
    /// Use constants from <see cref="TokenScope"/>.
    /// </param>
    /// <returns>The access token string.</returns>
    public static async Task<string> GetTokenAsync(string scopeKey)
    {
        if (s_tokenCache.TryGetValue(scopeKey, out var cachedToken))
        {
            return cachedToken;
        }

        var token = await AcquireTokenAsync(scopeKey);
        s_tokenCache[scopeKey] = token;
        return token;
    }

    /// <summary>
    /// Pre-warms the token cache for all provided scope keys.
    /// Call once in <c>OneTimeSetUp</c> to avoid per-test latency.
    /// </summary>
    /// <param name="scopeKeys">Scope keys to pre-load (use <see cref="TokenScope"/> constants).</param>
    public static async Task WarmUpAsync(IEnumerable<string> scopeKeys)
    {
        IEnumerable<Task<string>> tasks = scopeKeys.Select(GetTokenAsync);
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Clears the token cache. Useful for forcing token refresh in long-running runs.
    /// </summary>
    public static void ClearCache() => s_tokenCache.Clear();

    private static async Task<string> AcquireTokenAsync(string scopeKey)
    {
        if (!s_entraIdSettings.EntraIdScopes.TryGetValue(scopeKey, out var scope))
        {
            throw new KeyNotFoundException(
                $"Scope key '{scopeKey}' was not found in EntraIdSettings.EntraIdScopes. " +
                $"Available keys: {string.Join(", ", s_entraIdSettings.EntraIdScopes.Keys)}");
        }

        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
            .Create(s_entraIdSettings.EntraIdClientId)
            .WithClientSecret(s_entraIdSettings.EntraIdClientSecret)
            .WithAuthority(s_authority)
            .Build();

        AuthenticationResult result = await app
            .AcquireTokenForClient([scope])
            .ExecuteAsync();

        return result.AccessToken;
    }
}
