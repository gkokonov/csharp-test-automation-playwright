using Npgsql;

namespace CsharpTestAutomation.Tests.Database.NetBox.Queries;

internal static class NetBoxDatabase
{
    private const string DatabaseName = "netbox";

    public static T Run<T>(Func<NpgsqlConnection, T> query)
    {
        using NpgsqlConnection connection = PostgreSqlConnectionPool.Instance.GetConnection(DatabaseName);
        return query(connection);
    }
}
