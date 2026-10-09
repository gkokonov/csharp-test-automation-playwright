using CsharpTestAutomation.Bdd.Tests.Configurations;
using CsharpTestAutomation.Bdd.Tests.Configurations.Models;
using Npgsql;

namespace CsharpTestAutomation.Bdd.Tests.Database;

/// <summary>Run-owned data sources; each query acquires and releases its own connection.</summary>
public sealed class PostgreSqlConnectionPool(ExtendedConfiguration configuration) : IAsyncDisposable
{
    private readonly Dictionary<string, NpgsqlDataSource> _dataSources = [];

    public async Task InitializeAsync()
    {
        DBConfigurationDTO settings = configuration.DbSettings
            ?? throw new InvalidOperationException("DbSettings is not configured.");

        foreach (DatabaseInfo database in settings.Database)
        {
            string name = database.DbName ?? throw new InvalidOperationException("Database name is not configured.");
            var connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = settings.Server ?? throw new InvalidOperationException("Database server is not configured."),
                Port = settings.Port,
                Database = name,
                Username = database.Username ?? throw new InvalidOperationException("Database username is not configured."),
                Password = database.Password ?? throw new InvalidOperationException("Database password is not configured."),
                SslMode = settings.PostgreSql.EnableSslMode ? Enum.Parse<SslMode>(settings.PostgreSql.SslMode, true) : SslMode.Disable,
                Pooling = true,
                MinPoolSize = 1,
                MaxPoolSize = 20,
                CommandTimeout = configuration.DbQueryTimeoutSeconds
            };
            NpgsqlDataSource source = NpgsqlDataSource.Create(connectionString.ConnectionString);
            if (!_dataSources.TryAdd(name, source))
            {
                await source.DisposeAsync();
                throw new InvalidOperationException($"Duplicate database configuration: {name}.");
            }

            await using NpgsqlConnection connection = await source.OpenConnectionAsync();
        }
    }

    public ValueTask<NpgsqlConnection> GetConnectionAsync(string databaseName, CancellationToken cancellationToken = default) =>
        _dataSources.TryGetValue(databaseName, out NpgsqlDataSource? source)
            ? source.OpenConnectionAsync(cancellationToken)
            : throw new KeyNotFoundException($"Database '{databaseName}' is not configured.");

    public async ValueTask DisposeAsync()
    {
        List<Exception> errors = [];
        foreach (NpgsqlDataSource source in _dataSources.Values)
        {
            try
            {
                await source.DisposeAsync();
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }

        _dataSources.Clear();
        if (errors.Count > 0)
        {
            throw new AggregateException("Database source disposal failed.", errors);
        }
    }
}
