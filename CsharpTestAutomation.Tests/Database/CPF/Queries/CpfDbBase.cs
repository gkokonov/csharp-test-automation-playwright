using CsharpTestAutomation.Framework.Common;
using CsharpTestAutomation.Tests.Configurations;
using CsharpTestAutomation.Tests.Configurations.Models;
using Npgsql;

namespace CsharpTestAutomation.Tests.Database.CPF.Queries;

public static class CpfDbBase
{
    private static readonly ExtendedConfiguration s_configuration = AppConfiguration<ExtendedConfiguration>.Instance.Settings;
    private static readonly Lock s_lock = new();
    private static bool s_opened;

    public static NpgsqlConnection GetConnection()
    {
        EnsureConnectionsOpen();

        DBConfigurationDTO dbSettings = s_configuration.DbSettings
            ?? throw new InvalidOperationException(
                "DbSettings configuration section is missing. Configure 'DbSettings' in appsettings.");

        var databaseName = dbSettings.Database[0].DbName
            ?? throw new InvalidOperationException(
                "DbSettings.Database[0].DbName is not configured.");

        return PostgreSqlConnectionPool.Instance.GetConnection(databaseName);
    }

    private static void EnsureConnectionsOpen()
    {
        if (s_opened)
        {
            return;
        }

        lock (s_lock)
        {
            if (s_opened)
            {
                return;
            }

            PostgreSqlConnectionPool.Instance.OpenConnections();
            s_opened = true;
        }
    }
}
