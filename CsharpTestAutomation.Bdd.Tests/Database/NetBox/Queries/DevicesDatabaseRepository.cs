using CsharpTestAutomation.Bdd.Tests.Database.NetBox.DTO;
using Dapper;

namespace CsharpTestAutomation.Bdd.Tests.Database.NetBox.Queries;

public sealed class DevicesDatabaseRepository(PostgreSqlConnectionPool connections, int queryTimeoutSeconds)
{
    private readonly NetBoxDatabase _database = new(connections);

    public Task<DeviceRowDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id AS "Id", name AS "Name", site_id AS "SiteId",
                device_type_id AS "DeviceTypeId", role_id AS "RoleId",
                status AS "Status", description AS "Description"
            FROM dcim_device
            WHERE name = @Name
            """;

        return _database.RunAsync(connection => connection.QuerySingleOrDefaultAsync<DeviceRowDto>(
            new CommandDefinition(sql, new { Name = name }, commandTimeout: queryTimeoutSeconds,
                cancellationToken: cancellationToken)), cancellationToken);
    }
}
