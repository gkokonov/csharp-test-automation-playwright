using System.Collections.Concurrent;
using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Tests.Configurations;
using CsharpTestAutomation.Tests.Configurations.Models;
using NLog;
using Npgsql;

namespace CsharpTestAutomation.Tests.Database;

public sealed class PostgreSqlConnectionPool : IDisposable
{
    private static readonly Lazy<PostgreSqlConnectionPool> s_instance = new(() => new PostgreSqlConnectionPool());
    public static PostgreSqlConnectionPool Instance => s_instance.Value;

    private readonly ConcurrentDictionary<string, NpgsqlDataSource> _dataSources = new();
    private readonly Logger _log = LogManager.GetCurrentClassLogger();
    private static readonly ExtendedConfiguration s_configuration = AppConfiguration<ExtendedConfiguration>.Instance.Settings;
    private bool _disposed;

    private PostgreSqlConnectionPool()
    { }

    public void OpenConnections()
    {
        DBConfigurationDTO dbSettings = s_configuration.DbSettings
            ?? throw new InvalidOperationException(
                "DbSettings configuration section is missing. Configure 'DbSettings' in appsettings.");

        OpenConnections(dbSettings, _log);
    }

    public NpgsqlConnection GetConnection(string databaseName)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(PostgreSqlConnectionPool));

        if (_dataSources.TryGetValue(databaseName, out NpgsqlDataSource? dataSource))
        {
            NpgsqlConnection connection = dataSource.CreateConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            return connection;
        }

        throw new KeyNotFoundException($"Database '{databaseName}' not found in the configured data sources.");
    }

    public void CloseConnections()
    {
        foreach (NpgsqlDataSource dataSource in _dataSources.Values)
        {
            try
            {
                dataSource?.Dispose();
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Error disposing data source");
            }
        }

        _dataSources.Clear();
    }

    private void OpenConnections(DBConfigurationDTO dbSettings, Logger log)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(PostgreSqlConnectionPool));

        try
        {
            foreach (DatabaseInfo db in dbSettings.Database)
            {
                var dbName = db.DbName
                    ?? throw new InvalidOperationException(
                        "DbSettings.Database[].DbName is not configured.");

                NpgsqlDataSource dataSource = CreateDataSource(db, dbSettings);
                _dataSources.TryAdd(dbName, dataSource);

                // Verify connectivity once; callers acquire short-lived connections via GetConnection.
                using NpgsqlConnection connection = dataSource.CreateConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                log.Info($"PostgreSQL data source initialized for database: {dbName}");
            }
        }
        catch (PostgresException ex)
        {
            throw new Exception(
                "Error connecting to one or more PostgreSQL databases. \nPlease check your database connection settings or local AWS credentials file.",
                ex);
        }
    }

    private static NpgsqlDataSource CreateDataSource(DatabaseInfo dbInfo, DBConfigurationDTO dbSettings)
    {
        var connectionString = BuildConnectionString(dbInfo, dbSettings);
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        return dataSourceBuilder.Build();
    }

    private static string BuildConnectionString(DatabaseInfo dbInfo, DBConfigurationDTO dbSettings)
    {
        var connectionStringParts = new Dictionary<string, object> {
            ["Host"] = dbSettings.Server
                ?? throw new InvalidOperationException("DbSettings.Server is not configured."),
            ["Database"] = dbInfo.DbName
                ?? throw new InvalidOperationException("DbSettings.Database[].DbName is not configured."),
            ["Username"] = dbInfo.Username
                ?? throw new InvalidOperationException("DbSettings.Database[].Username is not configured."),
            ["Password"] = dbInfo.Password
                ?? throw new InvalidOperationException("DbSettings.Database[].Password is not configured."),
            ["Port"] = dbSettings.Port
        };

        if (dbSettings.PostgreSql.EnableSslMode)
        {
            connectionStringParts["SSL Mode"] = dbSettings.PostgreSql.SslMode;
            connectionStringParts["Trust Server Certificate"] = dbSettings.PostgreSql.TrustServerCertificate;
        }

        // Additional PostgreSQL specific settings
        connectionStringParts["Pooling"] = true;
        connectionStringParts["MinPoolSize"] = 1;
        connectionStringParts["MaxPoolSize"] = 20;
        connectionStringParts["Command Timeout"] = 30;

        var connectionString = string.Join(";", connectionStringParts.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return connectionString;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            CloseConnections();
            _disposed = true;
        }
    }
}
