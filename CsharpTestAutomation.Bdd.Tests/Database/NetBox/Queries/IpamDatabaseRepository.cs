using CsharpTestAutomation.Bdd.Tests.Database.NetBox.DTO;
using Dapper;

namespace CsharpTestAutomation.Bdd.Tests.Database.NetBox.Queries;

public sealed class IpamDatabaseRepository(PostgreSqlConnectionPool connections, int queryTimeoutSeconds)
{
    private readonly NetBoxDatabase _database = new(connections);

    public Task<PrefixRowDto?> GetPrefixByCidrAsync(string prefix, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id AS "Id", prefix::text AS "Prefix", status AS "Status", description AS "Description"
            FROM ipam_prefix
            WHERE prefix = @Prefix::cidr
            """;
        return _database.RunAsync(connection => connection.QuerySingleOrDefaultAsync<PrefixRowDto>(
            new CommandDefinition(sql, new { Prefix = prefix }, commandTimeout: queryTimeoutSeconds,
                cancellationToken: cancellationToken)), cancellationToken);
    }

    public Task<IpAddressRowDto?> GetIpAddressByAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id AS "Id", address::text AS "Address", status AS "Status", description AS "Description"
            FROM ipam_ipaddress
            WHERE address = @Address::inet
            """;
        return _database.RunAsync(connection => connection.QuerySingleOrDefaultAsync<IpAddressRowDto>(
            new CommandDefinition(sql, new { Address = address }, commandTimeout: queryTimeoutSeconds,
                cancellationToken: cancellationToken)), cancellationToken);
    }
}
