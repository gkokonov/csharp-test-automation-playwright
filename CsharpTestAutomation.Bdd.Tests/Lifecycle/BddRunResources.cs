using CsharpTestAutomation.Bdd.Tests.Authentication;
using CsharpTestAutomation.Bdd.Tests.Configurations;
using CsharpTestAutomation.Bdd.Tests.Database;
using CsharpTestAutomation.Bdd.Tests.Database.NetBox.Queries;
using CsharpTestAutomation.Bdd.Tests.UI.Pages;
using CsharpTestAutomation.Bdd.Tests.UI.Pages.NetBox;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Framework.UI;
using Microsoft.Playwright;

namespace CsharpTestAutomation.Bdd.Tests.Lifecycle;

/// <summary>Controlled run resources; scenario clients and browsers are never shared here.</summary>
public sealed class BddRunResources
{
    private static readonly Lazy<BddRunResources> s_current = new(() => new());
    private readonly HttpClient _authenticationHttpClient = new();
    private readonly SemaphoreSlim _databaseLock = new(1, 1);
    private readonly AuthenticationSnapshot _snapshot;
    private PostgreSqlConnectionPool? _connections;

    private BddRunResources()
    {
        Configuration = AppConfiguration<ExtendedConfiguration>.Instance.Settings;
        TokenSession = new(new NetBoxAuthClient(_authenticationHttpClient, Configuration.NetBox), Configuration.NetBox.ApiToken);
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            Configuration.Ui.StorageStateDirectory, $"netbox-bdd-{Guid.NewGuid():N}.json"));
        _snapshot = new(path);
    }

    public static BddRunResources Current => s_current.Value;

    public ExtendedConfiguration Configuration { get; }

    public NetBoxSession TokenSession { get; }

    public async Task<SitesDatabaseRepository> GetSitesRepositoryAsync() =>
        new(await GetConnectionsAsync(), Configuration.DbQueryTimeoutSeconds);

    public async Task<DevicesDatabaseRepository> GetDevicesRepositoryAsync() =>
        new(await GetConnectionsAsync(), Configuration.DbQueryTimeoutSeconds);

    private async Task<PostgreSqlConnectionPool> GetConnectionsAsync()
    {
        await _databaseLock.WaitAsync();
        try
        {
            if (_connections is null)
            {
                var connections = new PostgreSqlConnectionPool(Configuration);
                try
                {
                    await connections.InitializeAsync();
                    _connections = connections;
                }
                catch (Exception initializationError)
                {
                    try
                    {
                        await connections.DisposeAsync();
                    }
                    catch (Exception disposalError)
                    {
                        throw new AggregateException("Database setup and partial-source disposal failed.",
                            initializationError, disposalError);
                    }

                    throw;
                }
            }

            return _connections;
        }
        finally
        {
            _databaseLock.Release();
        }
    }

    public Task<string> GetStorageStateAsync() => _snapshot.GetPathAsync(CaptureAuthenticationAsync);

    public static async Task ReleaseAsync(Action<string, Exception> report)
    {
        List<Exception> errors = [];
        if (s_current.IsValueCreated)
        {
            BddRunResources resources = s_current.Value;
            await ScenarioLifecycle.AttemptAsync("authentication file", () => resources._snapshot.DisposeAsync().AsTask(), report, errors);
            if (resources._connections is not null)
            {
                await ScenarioLifecycle.AttemptAsync("database sources", () => resources._connections.DisposeAsync().AsTask(), report, errors);
            }

            await ScenarioLifecycle.AttemptAsync("token cache", () =>
            {
                resources.TokenSession.Dispose();
                return Task.CompletedTask;
            }, report, errors);
            await ScenarioLifecycle.AttemptAsync("authentication HTTP client", () =>
            {
                resources._authenticationHttpClient.Dispose();
                return Task.CompletedTask;
            }, report, errors);
            await ScenarioLifecycle.AttemptAsync("database lock", () =>
            {
                resources._databaseLock.Dispose();
                return Task.CompletedTask;
            }, report, errors);
        }

        await ScenarioLifecycle.AttemptAsync("Playwright process", PlaywrightBrowserFactory.DisposeAllAsync, report, errors);
        if (errors.Count > 0)
        {
            throw new AggregateException("Run resource disposal failed.", errors);
        }
    }

    private async Task CaptureAuthenticationAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(Configuration.NetBox.Username)
            || string.IsNullOrWhiteSpace(Configuration.NetBox.Password))
        {
            throw new InvalidOperationException("NetBox.Username and NetBox.Password are required for UI authentication.");
        }

        bool browserCreated = false;
        bool contextCreated = false;
        try
        {
            await Task.Run(() => Directory.CreateDirectory(Path.GetDirectoryName(path)!));
            await PlaywrightBrowserFactory.InitializeAsync();
            browserCreated = true;
            IBrowserContext context = await PlaywrightBrowserFactory.CreateContextAsync();
            contextCreated = true;
            IPage page = await PlaywrightBrowserFactory.CreatePageAsync();
            NetBoxLoginPage login = BaseUIPage.Create<NetBoxLoginPage>(page);
            await login.OpenAsync();
            await login.SignInAsync(Configuration.NetBox.Username, Configuration.NetBox.Password);
            await context.StorageStateAsync(new() { Path = path });
        }
        finally
        {
            // The bootstrap closes before the scenario's fresh browser is created.
            try
            {
                if (contextCreated)
                {
                    await PlaywrightBrowserFactory.DisposeContextAsync();
                }
            }
            finally
            {
                if (browserCreated)
                {
                    await PlaywrightBrowserFactory.DisposeBrowserAsync();
                }
            }
        }
    }
}
