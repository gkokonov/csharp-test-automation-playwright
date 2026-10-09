using Npgsql;

namespace CsharpTestAutomation.Bdd.Tests.Database.NetBox.Queries;

internal sealed class NetBoxDatabase(PostgreSqlConnectionPool connections)
{
    public async Task<T> RunAsync<T>(Func<NpgsqlConnection, Task<T>> query, CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await connections.GetConnectionAsync("netbox", cancellationToken);
        return await query(connection);
    }
}
