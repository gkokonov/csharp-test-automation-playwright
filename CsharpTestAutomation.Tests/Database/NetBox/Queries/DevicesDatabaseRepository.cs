using CsharpTestAutomation.Framework.DB;
using CsharpTestAutomation.Tests.Database.NetBox.DTO;
using Dapper;
using Npgsql;

namespace CsharpTestAutomation.Tests.Database.NetBox.Queries;

public static class DevicesDatabaseRepository
{
    public static DeviceRowDto? GetByName(NpgsqlConnection connection, string name)
    {
        const string sql = """
            SELECT id, name, site_id AS "SiteId", status
            FROM dcim_device
            WHERE name = @Name
            """;

        return DapperActions.Query<DeviceRowDto>(connection, sql, new DynamicParameters(new { Name = name }));
    }
}
