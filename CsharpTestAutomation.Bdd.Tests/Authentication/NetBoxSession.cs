namespace CsharpTestAutomation.Bdd.Tests.Authentication;

/// <summary>Run-owned token cache. Failed provisioning is not cached.</summary>
public sealed class NetBoxSession(NetBoxAuthClient authClient, string? configuredToken) : IDisposable
{
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _token = string.IsNullOrWhiteSpace(configuredToken) ? null : configuredToken;

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            _token ??= await authClient.ProvisionTokenAsync(cancellationToken);
            return _token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public void Dispose() => _tokenLock.Dispose();
}
