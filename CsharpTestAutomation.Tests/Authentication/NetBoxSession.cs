using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Tests.Configurations;
using CsharpTestAutomation.Tests.Configurations.Models;

namespace CsharpTestAutomation.Tests.Authentication;

public sealed class NetBoxSession
{
    private static readonly Lazy<NetBoxSession> s_default = new(CreateDefault);

    private readonly NetBoxAuthClient _authClient;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _token;

    public NetBoxSession(NetBoxAuthClient authClient, string? configuredToken)
    {
        _authClient = authClient;
        _token = string.IsNullOrWhiteSpace(configuredToken) ? null : configuredToken;
    }

    public static NetBoxSession Default => s_default.Value;

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_token is not null)
        {
            return _token;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            _token ??= await _authClient.ProvisionTokenAsync(cancellationToken);
            return _token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private static NetBoxSession CreateDefault()
    {
        NetBoxConfigurationDTO configuration = AppConfiguration<ExtendedConfiguration>.Instance.Settings.NetBox;
        return new(new NetBoxAuthClient(new HttpClient(), configuration), configuration.ApiToken);
    }
}
